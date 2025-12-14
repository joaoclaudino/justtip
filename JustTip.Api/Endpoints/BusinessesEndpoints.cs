using JustTip.Api.Common;
using JustTip.Api.Contracts.Businesses;
using JustTip.Domain.Entities;
using JustTip.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Api.Endpoints;

public static class BusinessesEndpoints
{
    public static IEndpointRouteBuilder MapBusinesses(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/businesses")
            .WithTags("Businesses");

        group.MapPost("/", CreateBusiness)
            .WithName("CreateBusiness")
            .Produces<BusinessResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", GetBusinesses)
            .WithName("GetBusinesses")
            .Produces<List<BusinessResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetBusinessById)
            .WithName("GetBusinessById")
            .Produces<BusinessResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateBusiness(
        CreateBusinessRequest request,
        JustTipDbContext db,
        CancellationToken ct)
    {
        var (ok, error) = Validation.RequiredString(request.Name, 200, "Name");
        if (!ok) return Validation.Problem400("Validation error", error!);

        var normalizedName = request.Name.Trim();

        var exists = await db.Businesses
            .AnyAsync(b => b.Name == normalizedName, ct);

        if (exists)
            return Validation.Problem409("Duplicate business", "A business with the same name already exists.");

        var business = new Business(normalizedName);

        db.Businesses.Add(business);
        await db.SaveChangesAsync(ct);

        var response = new BusinessResponse(business.Id, business.Name);
        return Results.Created($"/businesses/{business.Id}", response);
    }

    private static async Task<IResult> GetBusinesses(JustTipDbContext db, CancellationToken ct)
    {
        var list = await db.Businesses
            .OrderBy(b => b.Name)
            .Select(b => new BusinessResponse(b.Id, b.Name))
            .ToListAsync(ct);

        return Results.Ok(list);
    }

    private static async Task<IResult> GetBusinessById(Guid id, JustTipDbContext db, CancellationToken ct)
    {
        var business = await db.Businesses
            .Where(b => b.Id == id)
            .Select(b => new BusinessResponse(b.Id, b.Name))
            .FirstOrDefaultAsync(ct);

        return business is null
            ? Validation.Problem404("Not found", "Business not found.")
            : Results.Ok(business);
    }
}
