# Software Design Document (SDD): K-12 Attendance Module

## 1. Architectural Vision
Migrate the legacy VB6/SQL attendance module to a modern .NET 10 Web API.
* **Architecture:** Clean Architecture (API, Application, Domain, Infrastructure).
* **Database:** EF Core with `InMemory` provider (for zero-friction prototype evaluation).
* **Design-First:** API contracts must be strictly defined in OpenAPI format before implementation to allow OpenSpec validation.
* **Resilience:** All state-mutating endpoints must be strictly Idempotent (Upsert patterns).

## 2. Domain Schema & Ambiguity Resolution
The legacy system relied on missing tables. Implement the following reconstructed domain entities:
* `School`: SchoolId (PK), SchoolName, AbsenceAlertThreshold (Default: 10).
* `SchoolTerm`: TermId (PK), SchoolId, StartDate, EndDate.
* `Student`: StudentId (PK), SchoolId, FirstName, LastName, Grade.
* `AttendanceCode`: CodeValue (PK), Description, IsAbsent (bool), IsExcused (bool).
* `StudentAttendance`: AttendanceId (PK), StudentId, SchoolId, AttendDate, TermId, CodeValue, MinutesLate, Notes.
* `StudentAttendanceSummary`: SummaryId (PK), StudentId, SchoolYear (string), TotalAbsences.
* `StudentAlert`: AlertId (PK), StudentId, AlertType, SchoolYear, AlertMessage.

## 3. Core Endpoints & Business Logic

### POST /api/attendance/bulk
**Purpose:** Idempotent bulk ingestion of daily attendance.
* **Rule 1 (School Year):** If `AttendDate.Month >= 9`, SchoolYear is "YYYY-(YYYY+1)". Else, "(YYYY-1)-YYYY".
* **Rule 2 (Term):** Resolve `TermId` where `AttendDate` falls between the school's Term Start/End dates.
* **Rule 3 (Idempotent Upsert):** For each student, if an attendance record exists for the given `AttendDate`, update it. Otherwise, insert.
* **Rule 4 (Summary Recalculation):** Query `COUNT(*)` of all absences (`IsAbsent == true`) for the Student/Year. Upsert this count into `StudentAttendanceSummary`.
* **Rule 5 (Alerting):** If total absences >= `School.AbsenceAlertThreshold`, insert a 'CHRONIC_ABSENCE' alert in `StudentAlert` (only if one does not already exist for that year).

### GET /api/attendance/students/{studentId}/history
**Purpose:** Fetch detailed attendance history.

### GET /api/attendance/students/{studentId}/chronic-status
**Purpose:** Return `TotalAbsences`, `Threshold`, and a boolean `IsChronicallyAbsent`.

## 4. Execution Tasks for AI Agent
1. Generate the `/specs/openapi.yaml` contract based on these requirements. Wait for user approval.
2. Scaffold the .NET 10 solution and implement the EF Core InMemory context.
3. Implement the endpoints strictly adhering to the generated OpenAPI spec.
4. Generate a `requests.http` file with mock data to test the API locally.