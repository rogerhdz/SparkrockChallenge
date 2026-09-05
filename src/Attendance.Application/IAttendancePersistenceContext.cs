using Attendance.Domain;

namespace Attendance.Application;

public interface IAttendancePersistenceContext
{
    IQueryable<School> Schools { get; }
    IQueryable<SchoolTerm> SchoolTerms { get; }
    IQueryable<AttendanceCode> AttendanceCodes { get; }
    IQueryable<AttendanceRecord> AttendanceRecords { get; }
    IQueryable<StudentAttendanceSummary> StudentAttendanceSummaries { get; }
    IQueryable<StudentAlert> StudentAlerts { get; }
    IQueryable<AttendanceSubmissionLog> AttendanceSubmissionLogs { get; }

    Task<School?> GetSchoolAsync(int schoolId, CancellationToken cancellationToken = default);
    Task<AttendanceCode?> GetAttendanceCodeAsync(string codeValue, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetAttendanceRecordAsync(int studentId, int schoolId, DateTime attendDate, CancellationToken cancellationToken = default);
    Task AddAttendanceRecordAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
    Task AddAttendanceSubmissionLogAsync(AttendanceSubmissionLog log, CancellationToken cancellationToken = default);
    Task AddStudentAlertAsync(StudentAlert alert, CancellationToken cancellationToken = default);
    Task AddStudentSummaryAsync(StudentAttendanceSummary summary, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}