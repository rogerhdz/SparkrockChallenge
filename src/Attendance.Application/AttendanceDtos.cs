namespace Attendance.Application;

public class AttendanceBatchItemDto
{
    public string StudentId { get; set; } = string.Empty;
    public DateTime AttendDate { get; set; }
    public string AttendanceCode { get; set; } = "P";
    public int MinutesLate { get; set; }
    public string? Notes { get; set; }

    public int ParseStudentId()
    {
        if (string.IsNullOrWhiteSpace(StudentId))
        {
            throw new FormatException("StudentId is required.");
        }

        var normalized = StudentId.Trim();

        if (int.TryParse(normalized, out var directStudentId))
        {
            return directStudentId;
        }

        var digits = new string(normalized.Where(char.IsDigit).ToArray());
        if (digits.Length > 0 && int.TryParse(digits, out var parsedStudentId))
        {
            return parsedStudentId;
        }

        throw new FormatException($"StudentId '{StudentId}' is not a valid identifier.");
    }
}

public class AttendanceBatchRequestDto
{
    public int? SchoolId { get; set; }
    public List<AttendanceBatchItemDto> Records { get; set; } = new();
}

public class AttendanceBulkResultDto
{
    public int ProcessedCount { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int DuplicateRecordsResolved { get; set; }
}

public class StudentAttendanceHistoryItemDto
{
    public DateTime AttendDate { get; set; }
    public string AttendanceCode { get; set; } = string.Empty;
    public bool IsAbsent { get; set; }
    public bool IsExcused { get; set; }
    public int MinutesLate { get; set; }
    public string? Notes { get; set; }
    public string? TermName { get; set; }
}

public class StudentAttendanceHistoryResultDto
{
    public int StudentId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public IReadOnlyList<StudentAttendanceHistoryItemDto> History { get; set; } = Array.Empty<StudentAttendanceHistoryItemDto>();
    public int TotalAbsences { get; set; }
    public int AlertThreshold { get; set; }
    public bool IsChronicallyAbsent { get; set; }
}

public class ChronicStatusDto
{
    public int StudentId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public int TotalAbsences { get; set; }
    public int AlertThreshold { get; set; }
    public bool IsChronicallyAbsent { get; set; }
    public DateTime? LastUpdated { get; set; }
}
