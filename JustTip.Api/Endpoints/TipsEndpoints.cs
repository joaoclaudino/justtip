using JustTip.Api.Common;
using JustTip.Api.Contracts.Tips;
using JustTip.Application.Tips;
using JustTip.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Api.Endpoints;

public static class TipsEndpoints
{
    public static IEndpointRouteBuilder MapTips(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/businesses/{businessId:guid}/tips")
            .WithTags("Tips");

        group.MapPost("/{date}/distribute", DistributeTips)
            .WithName("DistributeTips")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static bool TryParseDate(string raw, out DateOnly date)
        => DateOnly.TryParse(raw, out date);

    private static async Task<IResult> DistributeTips(
        Guid businessId,
        string date,
        DistributeTipsRequest request,
        JustTipDbContext db,
        TipDistributionService distributor,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day))
            return Validation.Problem400("Validation error", "Date must be in a valid format (e.g. 2025-12-14).");

        if (request.TotalTips < 0)
            return Validation.Problem400("Validation error", "TotalTips must be >= 0.");

        var roster = await db.Rosters
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Date == day, ct);

        if (roster is null)
            return Validation.Problem404("Not found", "Roster not found for this date.");

        var entries = await db.RosterEntries
            .Where(e => e.RosterId == roster.Id)
            .Select(e => new { e.EmployeeId, e.HoursWorked })
            .ToListAsync(ct);

        try
        {
            var shares = distributor.Distribute(
                request.TotalTips,
                entries.Select(e => (e.EmployeeId, e.HoursWorked)).ToList()
            );

            var response = shares
                .OrderByDescending(s => s.HoursWorked)
                .Select(s => new TipShareResponse(s.EmployeeId, s.HoursWorked, s.TipAmount))
                .ToList();

            var totalAllocated = response.Sum(x => x.TipAmount);

            return Results.Ok(new
            {
                businessId,
                date = day.ToString("yyyy-MM-dd"),
                totalTips = request.TotalTips,
                totalAllocated,
                shares = response
            });
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is ArgumentException)
        {
            return Validation.Problem400("Distribution error", ex.Message);
        }
    }
}
