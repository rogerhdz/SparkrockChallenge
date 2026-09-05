using Attendance.Domain;

namespace Attendance.Application;

public class AttendanceCommandService
{
    private readonly IAttendancePersistenceContext _context;

    public AttendanceCommandService(IAttendancePersistenceContext context)
    {
        _context = context;
    }

    public async Task<AttendanceBulkResultDto> ProcessBatchAsync(int schoolId, IEnumerable<AttendanceBatchItemDto> records)
    {
        var school = await _context.GetSchoolAsync(schoolId)
            ?? throw new InvalidOperationException($"School {schoolId} was not found.");

        var items = records
            .Select(r => new
            {
                StudentId = r.ParseStudentId(),
                Record = r
            })
            .Where(r => r.StudentId > 0)
            .GroupBy(r => new { r.StudentId, Date = r.Record.AttendDate.Date })
            .Select(g => g.Last().Record)
            .ToList();

        var result = new AttendanceBulkResultDto
        {
            ProcessedCount = items.Count,
            DuplicateRecordsResolved = Math.Max(0, records.Count() - items.Count)
        };

        foreach (var item in items)
        {
            var studentId = item.ParseStudentId();
            var normalizedCode = (item.AttendanceCode ?? "P").Trim();
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                normalizedCode = "P";
            }

            var attendanceCode = await _context.GetAttendanceCodeAsync(normalizedCode)
                ?? new AttendanceCode
                {
                    CodeValue = normalizedCode,
                    Description = normalizedCode,
                    IsAbsent = normalizedCode != "P",
                    IsExcused = false,
                    IsActive = true
                };

            var record = await _context.GetAttendanceRecordAsync(studentId, schoolId, item.AttendDate.Date);

            if (record is null)
            {
                await _context.AddAttendanceRecordAsync(new AttendanceRecord
                {
                    StudentId = studentId,
                    SchoolId = schoolId,
                    AttendDate = item.AttendDate.Date,
                    AttendanceCode = attendanceCode.CodeValue,
                    IsAbsent = attendanceCode.IsAbsent,
                    IsExcused = attendanceCode.IsExcused,
                    MinutesLate = item.MinutesLate,
                    Notes = item.Notes,
                    CreatedAt = DateTime.UtcNow
                });

                result.InsertedCount++;
            }
            else
            {
                record.AttendanceCode = attendanceCode.CodeValue;
                record.IsAbsent = attendanceCode.IsAbsent;
                record.IsExcused = attendanceCode.IsExcused;
                record.MinutesLate = item.MinutesLate;
                record.Notes = item.Notes;
                record.ModifiedAt = DateTime.UtcNow;

                result.UpdatedCount++;
            }

            await _context.SaveChangesAsync();
            await RecalculateStudentSummaryAsync(schoolId, studentId, item.AttendDate.Date, school.AbsenceAlertThreshold);
        }

        await _context.AddAttendanceSubmissionLogAsync(new AttendanceSubmissionLog
        {
            SchoolId = schoolId,
            SubmittedDate = DateTime.UtcNow,
            AttendDate = DateTime.UtcNow,
            RecordCount = items.Count
        });

        await _context.SaveChangesAsync();

        return result;
    }

    public async Task ResolveAlertAsync(int studentId, int schoolId, string schoolYear)
    {
        var alert = _context.StudentAlerts
            .FirstOrDefault(a => a.StudentId == studentId
                && a.SchoolId == schoolId
                && a.SchoolYear == schoolYear
                && a.AlertType == "CHRONIC_ABSENCE"
                && a.ResolvedDate == null);

        if (alert is not null)
        {
            alert.ResolvedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task RecalculateStudentSummaryAsync(int schoolId, int studentId, DateTime attendDate, int threshold)
    {
        var schoolYear = SchoolYearCalculator.Calculate(attendDate);

        var totalAbsences = _context.AttendanceRecords
            .Count(r => r.StudentId == studentId
                && r.SchoolId == schoolId
                && SchoolYearCalculator.Calculate(r.AttendDate) == schoolYear
                && r.IsAbsent);

        var summary = _context.StudentAttendanceSummaries
            .FirstOrDefault(s => s.StudentId == studentId && s.SchoolId == schoolId && s.SchoolYear == schoolYear);

        if (summary is null)
        {
            await _context.AddStudentSummaryAsync(new StudentAttendanceSummary
            {
                StudentId = studentId,
                SchoolId = schoolId,
                SchoolYear = schoolYear,
                TotalAbsences = totalAbsences,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            summary.TotalAbsences = totalAbsences;
            summary.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        if (totalAbsences >= threshold)
        {
            var existingOpenAlert = _context.StudentAlerts
                .Any(alert => alert.StudentId == studentId
                    && alert.SchoolId == schoolId
                    && alert.AlertType == "CHRONIC_ABSENCE"
                    && alert.SchoolYear == schoolYear
                    && alert.ResolvedDate == null);

            if (!existingOpenAlert)
            {
                await _context.AddStudentAlertAsync(new StudentAlert
                {
                    StudentId = studentId,
                    SchoolId = schoolId,
                    AlertType = "CHRONIC_ABSENCE",
                    SchoolYear = schoolYear,
                    AlertDate = DateTime.UtcNow,
                    Message = $"Student has reached {totalAbsences} absences this year"
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}
