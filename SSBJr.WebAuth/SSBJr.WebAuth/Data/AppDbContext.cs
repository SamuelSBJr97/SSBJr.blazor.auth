using Microsoft.EntityFrameworkCore;
using SSBJr.WebAuth.Data.Models;

namespace SSBJr.WebAuth.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<TwoFactorLog> TwoFactorLogs { get; set; }
    public DbSet<UserObservation> UserObservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant
        modelBuilder.Entity<Tenant>()
            .HasKey(t => t.TenantId);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.Role)
            .HasDefaultValue("User");

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>()
            .HasKey(a => a.AuditId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TwoFactorLog
        modelBuilder.Entity<TwoFactorLog>()
            .HasKey(t => t.LogId);

        modelBuilder.Entity<TwoFactorLog>()
            .HasOne(t => t.User)
            .WithMany(u => u.TwoFactorLogs)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserObservation
        modelBuilder.Entity<UserObservation>()
            .HasKey(o => o.ObservationId);

        modelBuilder.Entity<UserObservation>()
            .HasOne(o => o.User)
            .WithMany(u => u.UserObservations)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserObservation>()
            .HasOne(o => o.Gestor)
            .WithMany()
            .HasForeignKey(o => o.GestorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
