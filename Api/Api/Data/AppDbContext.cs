using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<NorthAlert> NorthAlerts => Set<NorthAlert>();
    public DbSet<CenterAlert> CenterAlerts => Set<CenterAlert>();
    public DbSet<SouthAlert> SouthAlerts => Set<SouthAlert>();
    public DbSet<OverseasAlert> OverseasAlerts => Set<OverseasAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NorthAlert>()
            .ToTable("north_alerts");
        modelBuilder.Entity<CenterAlert>()
            .ToTable("center_alerts");
        modelBuilder.Entity<SouthAlert>()
            .ToTable("south_alerts");
        modelBuilder.Entity<OverseasAlert>()
            .ToTable("overseas_alerts");

        modelBuilder.Entity<NorthAlert>()
            .HasIndex(x => x.AlertId)
            .IsUnique();
        modelBuilder.Entity<CenterAlert>()
            .HasIndex(x => x.AlertId)
            .IsUnique();
        modelBuilder.Entity<SouthAlert>()
            .HasIndex(x => x.AlertId)
            .IsUnique();
        modelBuilder.Entity<OverseasAlert>()
            .HasIndex(x => x.AlertId)
            .IsUnique();
    }
}
