using JustTip.Api.Common;
using JustTip.Api.Contracts.Rosters;
using JustTip.Domain.Entities;
using JustTip.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Api.Endpoints;

public static class RostersEndpoints
{
    public static IEndpointRouteBuilder MapRosters(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/businesses/{businessId:guid}/rosters")
            .WithTags("Rosters");

        // Create (or get) roster for a given date
        group.MapPost("/{date}", CreateOrGetRoster)
            .WithName("CreateOrGetRoster")
            .Produces<RosterResponse>(StatusCodes.Status200OK)
            .Produces<RosterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Get roster + entries
        group.MapGet("/{date}", GetRosterWithEntries)
            .WithName("GetRosterWithEntries")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Upsert entry (employee hours) for that roster day
        group.MapPut("/{date}/entries", UpsertRosterEntry)
            .WithName("UpsertRosterEntry")
            .Produces<RosterEntryResponse>(StatusCodes.Status200OK)
            .Produces<RosterEntryResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static bool TryParseDate(string raw, out DateOnly date)
        => DateOnly.TryParse(raw, out date);

    private static async Task<IResult> CreateOrGetRoster(
        Guid businessId,
        string date,
        JustTipDbContext db,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day))
            return Validation.Problem400("Validation error", "Date must be in a valid format (e.g. 2025-12-14).");

        var businessExists = await db.Businesses.AnyAsync(b => b.Id == businessId, ct);
        if (!businessExists)
            return Validation.Problem404("Not found", "Business not found.");

        var roster = await db.Rosters
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Date == day, ct);

        if (roster is not null)
            return Results.Ok(new RosterResponse(roster.Id, roster.BusinessId, roster.Date.ToString("yyyy-MM-dd")));

        roster = new Roster(businessId, day);
        db.Rosters.Add(roster);
        await db.SaveChangesAsync(ct);

        var response = new RosterResponse(roster.Id, roster.BusinessId, roster.Date.ToString("yyyy-MM-dd"));
        return Results.Created($"/businesses/{businessId}/rosters/{response.Date}", response);
    }

    private static async Task<IResult> GetRosterWithEntries(
        Guid businessId,
        string date,
        JustTipDbContext db,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day))
            return Validation.Problem400("Validation error", "Date must be in a valid format (e.g. 2025-12-14).");

        var roster = await db.Rosters
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Date == day, ct);

        if (roster is null)
            return Validation.Problem404("Not found", "Roster not found for this date.");

        var entries = await db.RosterEntries
            .Where(e => e.RosterId == roster.Id)
            .OrderByDescending(e => e.HoursWorked)
            .Select(e => new RosterEntryResponse(e.Id, e.RosterId, e.EmployeeId, e.HoursWorked))
            .ToListAsync(ct);

        return Results.Ok(new
        {
            roster = new RosterResponse(roster.Id, roster.BusinessId, roster.Date.ToString("yyyy-MM-dd")),
            entries
        });
    }

    private static async Task<IResult> UpsertRosterEntry(
        Guid businessId,
        string date,
        UpsertRosterEntryRequest request,
        JustTipDbContext db,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day))
            return Validation.Problem400("Validation error", "Date must be in a valid format (e.g. 2025-12-14).");

        if (request.EmployeeId == Guid.Empty)
            return Validation.Problem400("Validation error", "EmployeeId is required.");

        if (request.HoursWorked < 0)
            return Validation.Problem400("Validation error", "HoursWorked must be >= 0.");

        // Check business + employee integrity
        var businessExists = await db.Businesses.AnyAsync(b => b.Id == businessId, ct);
        if (!businessExists)
            return Validation.Problem404("Not found", "Business not found.");

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, ct);
        if (employee is null)
            return Validation.Problem404("Not found", "Employee not found.");

        if (employee.BusinessId != businessId)
            return Validation.Problem409("Invalid employee", "Employee does not belong to this business.");

        // Ensure roster exists (create if missing)
        var roster = await db.Rosters
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Date == day, ct);

        if (roster is null)
        {
            roster = new Roster(businessId, day);
            db.Rosters.Add(roster);
            await db.SaveChangesAsync(ct);
        }

        var existing = await db.RosterEntries
            .FirstOrDefaultAsync(e => e.RosterId == roster.Id && e.EmployeeId == request.EmployeeId, ct);

        if (existing is null)
        {
            var entry = new RosterEntry(roster.Id, request.EmployeeId, request.HoursWorked);
            db.RosterEntries.Add(entry);
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/businesses/{businessId}/rosters/{day:yyyy-MM-dd}",
                new RosterEntryResponse(entry.Id, entry.RosterId, entry.EmployeeId, entry.HoursWorked));
        }

        existing.SetHours(request.HoursWorked);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new RosterEntryResponse(existing.Id, existing.RosterId, existing.EmployeeId, existing.HoursWorked));
    }
}
