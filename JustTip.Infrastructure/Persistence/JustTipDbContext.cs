using JustTip.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Persistence;

public sealed class JustTipDbContext : DbContext
{
    public JustTipDbContext(DbContextOptions<JustTipDbContext> options)
        : base(options) { }

    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Roster> Rosters => Set<Roster>();
    public DbSet<RosterEntry> RosterEntries => Set<RosterEntry>();


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

        modelBuilder.Entity<Roster>(r =>
        {
            r.ToTable("Rosters");
            r.HasKey(x => x.Id);

            r.Property(x => x.BusinessId).IsRequired();

            // SQLite doesn't have a native DateOnly type: store as TEXT (ISO yyyy-MM-dd)
            r.Property(x => x.Date)
                .HasConversion(
                    v => v.ToString("yyyy-MM-dd"),
                    v => DateOnly.Parse(v))
                .IsRequired();

            r.HasIndex(x => new { x.BusinessId, x.Date })
                .IsUnique();
        });

        modelBuilder.Entity<RosterEntry>(e =>
        {
            e.ToTable("RosterEntries");
            e.HasKey(x => x.Id);

            e.Property(x => x.HoursWorked)
                .HasPrecision(9, 2)
                .IsRequired();

            e.HasIndex(x => new { x.RosterId, x.EmployeeId })
                .IsUnique();

            e.HasOne<Roster>()
                .WithMany()
                .HasForeignKey(x => x.RosterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee FK just to enforce integrity
            e.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
