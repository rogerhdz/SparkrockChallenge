# Legacy Schema Inferences
The legacy `schema.sql` is missing dependencies used in the Stored Procedures. You MUST scaffold these inferred entities:
- `School`: SchoolId (PK), SchoolName, AbsenceAlertThreshold (Default: 10).
- `SchoolTerm`: TermId (PK), SchoolId, StartDate, EndDate.
- `AttendanceSubmissionLog`: LogId (PK), SchoolId, SubmittedDate, AttendDate, RecordCount.