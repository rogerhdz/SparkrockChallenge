using Attendance.Application;
using Attendance.Domain;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure;

public class AttendancePersistenceContext : IAttendancePersistenceContext
{
    private readonly AttendanceDbContext _dbContext;

    public AttendancePersistenceContext(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<School> Schools => _dbContext.Schools;
    public IQueryable<SchoolTerm> SchoolTerms => _dbContext.SchoolTerms;
    public IQueryable<AttendanceCode> AttendanceCodes => _dbContext.AttendanceCodes;
    public IQueryable<AttendanceRecord> AttendanceRecords => _dbContext.AttendanceRecords;
    public IQueryable<StudentAttendanceSummary> StudentAttendanceSummaries => _dbContext.StudentAttendanceSummaries;
    public IQueryable<StudentAlert> StudentAlerts => _dbContext.StudentAlerts;
    public IQueryable<AttendanceSubmissionLog> AttendanceSubmissionLogs => _dbContext.AttendanceSubmissionLogs;

    public Task<School?> GetSchoolAsync(int schoolId, CancellationToken cancellationToken = default)
        => _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);

    public Task<AttendanceCode?> GetAttendanceCodeAsync(string codeValue, CancellationToken cancellationToken = default)
        => _dbContext.AttendanceCodes.FirstOrDefaultAsync(code => code.CodeValue == codeValue && code.IsActive, cancellationToken);

    public Task<AttendanceRecord?> GetAttendanceRecordAsync(int studentId, int schoolId, DateTime attendDate, CancellationToken cancellationToken = default)
        => _dbContext.AttendanceRecords.FirstOrDefaultAsync(r => r.StudentId == studentId && r.SchoolId == schoolId && r.AttendDate.Date == attendDate.Date, cancellationToken);

    public Task AddAttendanceRecordAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.AttendanceRecords.Add(record);
        return Task.CompletedTask;
    }

    public Task AddAttendanceSubmissionLogAsync(AttendanceSubmissionLog log, CancellationToken cancellationToken = default)
    {
        _dbContext.AttendanceSubmissionLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task AddStudentAlertAsync(StudentAlert alert, CancellationToken cancellationToken = default)
    {
        _dbContext.StudentAlerts.Add(alert);
        return Task.CompletedTask;
    }

    public Task AddStudentSummaryAsync(StudentAttendanceSummary summary, CancellationToken cancellationToken = default)
    {
        _dbContext.StudentAttendanceSummaries.Add(summary);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
