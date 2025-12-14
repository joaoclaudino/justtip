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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
