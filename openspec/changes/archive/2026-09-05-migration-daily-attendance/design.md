## Context

See proposal.md for the motivation. This design addresses how the attendance migration will be implemented within the repo’s Clean Architecture constraints and the legacy SQL rules inferred from the VB6 module.

The project will use a .NET 8 Web API with four layers: Domain, Application, Infrastructure, and API. The Attendance capability is a focused feature area under the `api-attendance` domain and will be implemented as a CQRS-oriented service layer: commands handle ingestion and mutation, while queries handle history and chronic-status lookups. Persistence will use EF Core InMemory for deterministic testability and local validation.

## Goals / Non-Goals

**Goals:**
- Replace the XML-based stored-procedure model with a typed JSON batch endpoint.
- Preserve legacy business logic for school-year creation, term resolution, and absentee thresholds.
- Keep attendance ingestion idempotent and safe for repeated submissions.
- Make chronic absenteeism calculations and alert generation explicit and testable.
- Support the reporting and query behaviors expected by the legacy module.

**Non-Goals:**
- No real SQL Server implementation in this phase.
- No print/reporting integration beyond the API contract.
- No broad change to unrelated K-12 modules.

## Decisions

### 1. JSON batch input, not XML
The legacy form emits XML; the replacement API will accept a standard JSON array of attendance records. This avoids leaking legacy naming conventions into the domain model and aligns with modern ASP.NET Core input contracts. The payload shape will be a list of records with `studentId`, `attendDate`, `attendanceCode`, `minutesLate`, and `notes`.

**Alternative considered:** Keep the XML contract behind a translation layer. This was rejected because it preserves a brittle legacy contract and makes modern validation and testability weaker.

### 2. Last record wins within a batch
When multiple entries for the same student and date appear in the same bulk request, the last entry wins. This matches the clarified requirement and makes batch semantics deterministic for duplicate student-date pairs during bulk upserts.

**Alternative considered:** Reject duplicates or aggregate them. Rejected because the requirement explicitly defines deterministic overwrite semantics and the legacy procedure effectively updates existing attendance rows rather than creating duplicates.

### 3. Idempotency key is composite: student + attend date
The idempotency mechanism for bulk ingestion will rely on a composite identity of `StudentId + AttendDate`. During upsert, the application will resolve each batch record to that key and update the existing row if present; otherwise it inserts a new one. This prevents duplicate absence counting during repeated submissions and matches the VB6 stored procedure’s update-or-insert behavior.

**Alternative considered:** Use client-supplied identifiers or a global request hash. Rejected because the migration uses the natural business identity of student/date and is simpler to reason about in the domain.

### 4. School-year logic stays September-based
The school year is computed as:
- September through December belong to `YYYY-(YYYY+1)`
- January through August belong to `(YYYY-1)-YYYY`
This mirrors the legacy stored-procedure rule and the business context.

**Alternative considered:** Calendar-year grouping. Rejected because the domain requirement and existing SQL logic explicitly anchor the school year to the academic year.

### 5. Alert lifecycle is active-unresolved only
An alert is active only when it remains unresolved for the current school year. Once a chronic absence alert has been resolved, a new one can be generated later in the same year if the threshold is crossed again. This matches the clarified alert-resolution rule.

**Alternative considered:** Prevent all future generation in a year after one alert resolves. Rejected because the rule requires re-triggering once the threshold is crossed again.

### 6. CQRS in the Application layer
The Application layer will be split logically into command and query concerns:
- Commands: bulk attendance ingestion, summary recomputation, alert creation
- Queries: student attendance history and chronic status evaluation
This preserves the separation required by the project’s architecture rules and keeps the API controllers thin.

### 7. Persistence and data boundaries
EF Core InMemory is the persistence strategy for the migration phase. Data boundaries are maintained by explicit domain entities and infrastructure persistence models, with all DB access behind repository or DbContext abstractions. This gives robust testability while keeping the business rules in the Application layer rather than in controllers or the API project.

**Alternative considered:** Direct SQL Server integration in the API layer. Rejected because the project explicitly requires EF Core InMemory and Clean Architecture separation.

## Risks / Trade-offs

- [Risk] Legacy SQL logic is not fully specified in the schema and depends on inferred entities. → Mitigation: encode the required School, SchoolTerm, AttendanceSubmissionLog assumptions into the domain and infrastructure contracts for this migration.
- [Risk] Duplicate records inside a batch could produce non-obvious final state. → Mitigation: define and test last-entry-wins semantics explicitly.
- [Risk] Chronic-absence thresholds may shift by school. → Mitigation: resolve the threshold from the school configuration and keep it scoped to the institution and school year.
- [Risk] Re-running ingestion may update records unexpectedly. → Mitigation: treat the request as an upsert on `studentId + attendDate` and verify with tests for repeated submissions.

## Migration Plan

1. Scaffold the .NET 8 solution and Clean Architecture project structure.
2. Add the required EF Core InMemory and xUnit dependencies.
3. Model the attendance domain and supporting entities for school year, term, attendance, summary, and alert state.
4. Implement the bulk ingestion command and query services in the Application layer.
5. Implement EF Core persistence and DB context configuration in Infrastructure.
6. Expose minimal API endpoints in the API layer for bulk ingestion and reporting.
7. Add xUnit tests for school-year, threshold, idempotency, and duplicate-batch scenarios.
8. Run build and tests to confirm the threshold and coverage requirements.

## Open Questions

None at this stage. The clarified requirements and the inferred legacy SQL behavior are sufficient to proceed with the ensuing specification and tasking phases.
