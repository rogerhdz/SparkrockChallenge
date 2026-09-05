using Attendance.Application;
using Attendance.Domain;
using Attendance.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Attendance.UnitTests;

public class AttendanceServiceTests
{
    private static AttendanceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AttendanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AttendanceDbContext(options);

        context.Schools.Add(new School { Id = 1, Name = "North Elementary", AbsenceAlertThreshold = 10 });
        context.SchoolTerms.Add(new SchoolTerm { Id = 1, SchoolId = 1, Name = "Fall", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 1, 31) });
        context.AttendanceCodes.Add(new AttendanceCode { Id = 1, CodeValue = "P", Description = "Present", IsAbsent = false, IsExcused = false, IsActive = true });
        context.AttendanceCodes.Add(new AttendanceCode { Id = 2, CodeValue = "A", Description = "Absent", IsAbsent = true, IsExcused = false, IsActive = true });
        context.SaveChanges();

        return context;
    }

    private static IAttendancePersistenceContext CreatePersistenceContext(AttendanceDbContext context)
    {
        return new AttendancePersistenceContext(context);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenDuplicateStudentDateExists_LastEntryWins()
    {
        await using var context = CreateContext();
        var persistence = CreatePersistenceContext(context);
        var service = new AttendanceCommandService(persistence);

        var records = new[]
        {
            new AttendanceBatchItemDto { StudentId = "STU-101", AttendDate = new DateTime(2025, 9, 12), AttendanceCode = "A", MinutesLate = 0, Notes = "First" },
            new AttendanceBatchItemDto { StudentId = "STU-101", AttendDate = new DateTime(2025, 9, 12), AttendanceCode = "P", MinutesLate = 5, Notes = "Last" },
        };

        var result = await service.ProcessBatchAsync(1, records);

        result.ProcessedCount.Should().Be(1);
        result.DuplicateRecordsResolved.Should().Be(1);

        var saved = await context.AttendanceRecords.SingleAsync();
        saved.AttendanceCode.Should().Be("P");
        saved.MinutesLate.Should().Be(5);
        saved.Notes.Should().Be("Last");
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenAbsenceThresholdReached_CreatesChronicAbsenceAlert()
    {
        await using var context = CreateContext();
        var persistence = CreatePersistenceContext(context);
        var service = new AttendanceCommandService(persistence);

        for (var i = 0; i < 10; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                StudentId = 202,
                SchoolId = 1,
                AttendDate = new DateTime(2025, 9, 10 + i),
                AttendanceCode = "A",
                IsAbsent = true,
                IsExcused = false,
                MinutesLate = 0,
                Notes = "Absent"
            });
        }

        context.SaveChanges();

        var result = await service.ProcessBatchAsync(1, new[]
        {
            new AttendanceBatchItemDto { StudentId = "STU-202", AttendDate = new DateTime(2025, 9, 20), AttendanceCode = "A", MinutesLate = 0, Notes = "Late threshold" }
        });

        result.ProcessedCount.Should().Be(1);
        (await context.StudentAlerts.AnyAsync(a => a.StudentId == 202 && a.SchoolYear == "2025-2026" && a.AlertType == "CHRONIC_ABSENCE")).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenAlertResolved_AllowsRecreationWhenThresholdReachedAgain()
    {
        await using var context = CreateContext();
        var persistence = CreatePersistenceContext(context);
        var service = new AttendanceCommandService(persistence);

        var alert = new StudentAlert
        {
            StudentId = 303,
            SchoolId = 1,
            AlertType = "CHRONIC_ABSENCE",
            SchoolYear = "2025-2026",
            AlertDate = DateTime.UtcNow,
            Message = "Existing",
            ResolvedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.StudentAlerts.Add(alert);

        for (var i = 0; i < 9; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                StudentId = 303,
                SchoolId = 1,
                AttendDate = new DateTime(2025, 9, 1 + i),
                AttendanceCode = "A",
                IsAbsent = true,
                IsExcused = false,
                MinutesLate = 0,
                Notes = "Prior absence"
            });
        }
        context.SaveChanges();

        await service.ProcessBatchAsync(1, new[]
        {
            new AttendanceBatchItemDto { StudentId = "STU-303", AttendDate = new DateTime(2025, 9, 15), AttendanceCode = "A", MinutesLate = 0, Notes = "Trigger" }
        });

        var openAlerts = await context.StudentAlerts
            .Where(a => a.StudentId == 303 && a.SchoolYear == "2025-2026" && a.AlertType == "CHRONIC_ABSENCE" && a.ResolvedDate == null)
            .CountAsync();

        openAlerts.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void SchoolYearCalculator_ShouldUseSeptemberBoundary()
    {
        SchoolYearCalculator.Calculate(new DateTime(2025, 9, 1)).Should().Be("2025-2026");
        SchoolYearCalculator.Calculate(new DateTime(2026, 8, 31)).Should().Be("2025-2026");
        SchoolYearCalculator.Calculate(new DateTime(2026, 1, 15)).Should().Be("2025-2026");
        SchoolYearCalculator.Calculate(new DateTime(2025, 8, 15)).Should().Be("2024-2025");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnRecordedAbsencesForSchoolYear()
    {
        await using var context = CreateContext();
        var persistence = CreatePersistenceContext(context);
        var service = new AttendanceQueryService(persistence);

        context.AttendanceRecords.AddRange(
            new AttendanceRecord { StudentId = 501, SchoolId = 1, AttendDate = new DateTime(2025, 9, 10), AttendanceCode = "A", IsAbsent = true, IsExcused = false, MinutesLate = 0, Notes = "Absent 1" },
            new AttendanceRecord { StudentId = 501, SchoolId = 1, AttendDate = new DateTime(2025, 9, 12), AttendanceCode = "P", IsAbsent = false, IsExcused = false, MinutesLate = 0, Notes = "Present" }
        );
        context.StudentAttendanceSummaries.Add(new StudentAttendanceSummary { StudentId = 501, SchoolId = 1, SchoolYear = "2025-2026", TotalAbsences = 1, LastUpdated = DateTime.UtcNow });
        context.SaveChanges();

        var result = await service.GetHistoryAsync(501, 1, "2025-2026");

        result.History.Should().HaveCount(2);
        result.TotalAbsences.Should().Be(1);
        result.IsChronicallyAbsent.Should().BeFalse();
    }

    [Fact]
    public async Task GetChronicStatusAsync_ShouldMarkChronicallyAbsentWhenThresholdReached()
    {
        await using var context = CreateContext();
        var persistence = CreatePersistenceContext(context);
        var service = new AttendanceQueryService(persistence);

        context.StudentAttendanceSummaries.Add(new StudentAttendanceSummary { StudentId = 601, SchoolId = 1, SchoolYear = "2025-2026", TotalAbsences = 10, LastUpdated = DateTime.UtcNow });
        context.SaveChanges();

        var result = await service.GetChronicStatusAsync(601, 1, "2025-2026");

        result.TotalAbsences.Should().Be(10);
        result.IsChronicallyAbsent.Should().BeTrue();
        result.AlertThreshold.Should().Be(10);
    }
}
