using Attendance.Domain;

namespace Attendance.Application;

public class AttendanceQueryService
{
    private readonly IAttendancePersistenceContext _context;

    public AttendanceQueryService(IAttendancePersistenceContext context)
    {
        _context = context;
    }

    public Task<StudentAttendanceHistoryResultDto> GetHistoryAsync(int studentId, int schoolId, string? schoolYear = null)
    {
        var resolvedSchoolYear = schoolYear ?? SchoolYearCalculator.Calculate(DateTime.UtcNow);

        var records = _context.AttendanceRecords
            .Where(r => r.StudentId == studentId && r.SchoolId == schoolId)
            .OrderByDescending(r => r.AttendDate)
            .ToList();

        var filtered = records
            .Where(record => SchoolYearCalculator.Calculate(record.AttendDate) == resolvedSchoolYear)
            .Select(record => new StudentAttendanceHistoryItemDto
            {
                AttendDate = record.AttendDate,
                AttendanceCode = record.AttendanceCode,
                IsAbsent = record.IsAbsent,
                IsExcused = record.IsExcused,
                MinutesLate = record.MinutesLate,
                Notes = record.Notes,
                TermName = _context.SchoolTerms
                    .Where(term => term.Id == record.TermId)
                    .Select(term => term.Name)
                    .FirstOrDefault()
            })
            .ToList();

        var summary = _context.StudentAttendanceSummaries
            .FirstOrDefault(s => s.StudentId == studentId && s.SchoolId == schoolId && s.SchoolYear == resolvedSchoolYear);

        var school = _context.Schools.FirstOrDefault(s => s.Id == schoolId)
            ?? throw new InvalidOperationException($"School {schoolId} was not found.");

        return Task.FromResult(new StudentAttendanceHistoryResultDto
        {
            StudentId = studentId,
            SchoolYear = resolvedSchoolYear,
            History = filtered,
            TotalAbsences = summary?.TotalAbsences ?? 0,
            AlertThreshold = school.AbsenceAlertThreshold,
            IsChronicallyAbsent = summary is not null && summary.TotalAbsences >= school.AbsenceAlertThreshold
        });
    }

    public Task<ChronicStatusDto> GetChronicStatusAsync(int studentId, int schoolId, string? schoolYear = null)
    {
        var resolvedSchoolYear = schoolYear ?? SchoolYearCalculator.Calculate(DateTime.UtcNow);

        var summary = _context.StudentAttendanceSummaries
            .FirstOrDefault(s => s.StudentId == studentId && s.SchoolId == schoolId && s.SchoolYear == resolvedSchoolYear);

        var school = _context.Schools.FirstOrDefault(s => s.Id == schoolId)
            ?? throw new InvalidOperationException($"School {schoolId} was not found.");

        return Task.FromResult(new ChronicStatusDto
        {
            StudentId = studentId,
            SchoolYear = resolvedSchoolYear,
            TotalAbsences = summary?.TotalAbsences ?? 0,
            AlertThreshold = school.AbsenceAlertThreshold,
            IsChronicallyAbsent = summary is not null && summary.TotalAbsences >= school.AbsenceAlertThreshold,
            LastUpdated = summary?.LastUpdated
        });
    }
}
