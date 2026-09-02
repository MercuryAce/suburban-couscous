using Microsoft.EntityFrameworkCore;
using VisitManagement.Infrastructure.Persistence.Entities;

namespace VisitManagement.Infrastructure.Persistence;

public class VisitManagementDbContext : DbContext
{
    public VisitManagementDbContext(DbContextOptions<VisitManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<VisitRecord> Visits => Set<VisitRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var visit = modelBuilder.Entity<VisitRecord>();
        visit.ToTable("Visits");
        visit.HasKey(v => v.Id);
        visit.Property(v => v.Status).HasConversion<string>().HasMaxLength(32);
        visit.Property(v => v.VehicleLicenceNumber).HasMaxLength(32).IsRequired();
        visit.Property(v => v.VisitorId).HasMaxLength(64).IsRequired();
        visit.Property(v => v.VisitorFirstName).HasMaxLength(100).IsRequired();
        visit.Property(v => v.VisitorLastName).HasMaxLength(100).IsRequired();
        visit.Property(v => v.CreatedBy).HasMaxLength(128).IsRequired();
        visit.Property(v => v.UpdatedBy).HasMaxLength(128).IsRequired();
        visit.HasIndex(v => v.CreatedAt);
        visit.HasMany(v => v.Activities)
            .WithOne()
            .HasForeignKey(a => a.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        var activity = modelBuilder.Entity<ActivityRecord>();
        activity.ToTable("Activities");
        activity.HasKey(a => a.Id);
        activity.Property(a => a.Type).HasConversion<string>().HasMaxLength(32);
        activity.Property(a => a.TravellerNumber).HasMaxLength(64).IsRequired();
    }
}
