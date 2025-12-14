using JustTip.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Persistence;

public sealed class JustTipDbContext : DbContext
{
    public JustTipDbContext(DbContextOptions<JustTipDbContext> options)
        : base(options) { }

    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(b =>
        {
            b.ToTable("Businesses");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Employees");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);

            e.HasOne<Business>()
                .WithMany()
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.BusinessId, x.Name });
        });
    }
}
