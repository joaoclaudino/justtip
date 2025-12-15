using JustTip.Api.Endpoints;
using JustTip.Application.Tips;
using JustTip.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core + SQLite
builder.Services.AddDbContext<JustTipDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("JustTipDb");
    options.UseSqlite(cs);
});

builder.Services.AddSingleton<TipDistributionService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapBusinesses();
app.MapEmployees();
app.MapRosters();
app.MapTips();


app.Run();
