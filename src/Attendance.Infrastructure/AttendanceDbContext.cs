using Attendance.Domain;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure;

public class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolTerm> SchoolTerms => Set<SchoolTerm>();
    public DbSet<AttendanceCode> AttendanceCodes => Set<AttendanceCode>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<StudentAttendanceSummary> StudentAttendanceSummaries => Set<StudentAttendanceSummary>();
    public DbSet<StudentAlert> StudentAlerts => Set<StudentAlert>();
    public DbSet<AttendanceSubmissionLog> AttendanceSubmissionLogs => Set<AttendanceSubmissionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<SchoolTerm>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.StartDate).IsRequired();
            entity.Property(x => x.EndDate).IsRequired();
        });

        modelBuilder.Entity<AttendanceCode>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodeValue).IsRequired();
            entity.Property(x => x.Description).IsRequired();
            entity.HasIndex(x => x.CodeValue).IsUnique();
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AttendanceCode).IsRequired();
            entity.HasIndex(x => new { x.StudentId, x.SchoolId, x.AttendDate }).IsUnique();
        });

        modelBuilder.Entity<StudentAttendanceSummary>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.StudentId, x.SchoolId, x.SchoolYear }).IsUnique();
        });

        modelBuilder.Entity<StudentAlert>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AlertType).IsRequired();
            entity.Property(x => x.SchoolYear).IsRequired();
        });

        modelBuilder.Entity<AttendanceSubmissionLog>(entity =>
        {
            entity.HasKey(x => x.Id);
        });
    }
}
