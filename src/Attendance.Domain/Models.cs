namespace Attendance.Domain;

public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AbsenceAlertThreshold { get; set; } = 10;
}

public class SchoolTerm
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AttendanceCode
{
    public int Id { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAbsent { get; set; }
    public bool IsExcused { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SchoolId { get; set; }
    public DateTime AttendDate { get; set; }
    public int? TermId { get; set; }
    public string AttendanceCode { get; set; } = string.Empty;
    public bool IsAbsent { get; set; }
    public bool IsExcused { get; set; }
    public int MinutesLate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
}

public class StudentAttendanceSummary
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SchoolId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public int TotalAbsences { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class StudentAlert
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SchoolId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
    public DateTime? ResolvedDate { get; set; }
}

public class AttendanceSubmissionLog
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public DateTime AttendDate { get; set; }
    public int RecordCount { get; set; }
}

public static class SchoolYearCalculator
{
    public static string Calculate(DateTime attendDate)
    {
        var year = attendDate.Year;

        if (attendDate.Month >= 9)
        {
            return $"{year}-{year + 1}";
        }

        return $"{year - 1}-{year}";
    }
}
