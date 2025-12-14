using JustTip.Api.Common;
using JustTip.Api.Contracts.Employees;
using JustTip.Domain.Entities;
using JustTip.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Api.Endpoints;

public static class EmployeesEndpoints
{
    public static IEndpointRouteBuilder MapEmployees(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/businesses/{businessId:guid}/employees")
            .WithTags("Employees");

        group.MapPost("/", CreateEmployee)
            .WithName("CreateEmployee")
            .Produces<EmployeeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", GetEmployeesByBusiness)
            .WithName("GetEmployeesByBusiness")
            .Produces<List<EmployeeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateEmployee(
        Guid businessId,
        CreateEmployeeRequest request,
        JustTipDbContext db,
        CancellationToken ct)
    {
        var businessExists = await db.Businesses.AnyAsync(b => b.Id == businessId, ct);
        if (!businessExists)
            return Validation.Problem404("Not found", "Business not found.");

        var (ok, error) = Validation.RequiredString(request.Name, 200, "Name");
        if (!ok) return Validation.Problem400("Validation error", error!);

        var normalizedName = request.Name.Trim();

        var exists = await db.Employees.AnyAsync(
            e => e.BusinessId == businessId && e.Name == normalizedName,
            ct);

        if (exists)
            return Validation.Problem409("Duplicate employee", "An employee with the same name already exists for this business.");

        var employee = new Employee(businessId, normalizedName);

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        var response = new EmployeeResponse(employee.Id, employee.BusinessId, employee.Name);
        return Results.Created($"/businesses/{businessId}/employees/{employee.Id}", response);
    }

    private static async Task<IResult> GetEmployeesByBusiness(Guid businessId, JustTipDbContext db, CancellationToken ct)
    {
        var businessExists = await db.Businesses.AnyAsync(b => b.Id == businessId, ct);
        if (!businessExists)
            return Validation.Problem404("Not found", "Business not found.");

        var list = await db.Employees
            .Where(e => e.BusinessId == businessId)
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeResponse(e.Id, e.BusinessId, e.Name))
            .ToListAsync(ct);

        return Results.Ok(list);
    }
}
