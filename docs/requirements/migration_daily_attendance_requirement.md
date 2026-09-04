# MIGRATION REQUIREMENT: K-12 Legacy VB6/SQL Attendance Module to .NET 8

## Objective
Migrate the legacy desktop attendance module (`frmDailyAttendance.frm` and associated stored procedures) into a modern, resilient .NET 8 Web API following Clean Architecture principles and Spec-Driven Development (SDD).

## Scope of Work
1. **Bulk Ingestion Endpoint (`POST /api/attendance/bulk`):**
   - Must handle batch payloads efficiently (replacing the legacy XML approach).
   - Enforce strict **Idempotency** using upsert patterns via EF Core (utilizing InMemory provider for testing).
   - Implement business rules for School Year calculation (September-based) and Term resolution.
   - Recalculate chronic absenteeism dynamically (`COUNT` of absences) and evaluate the threshold (default: 10) to generate alerts idempotently.
2. **Query Endpoints:**
   - History endpoint for student attendance.
   - Chronic status evaluation endpoint.
3. **Quality & Testing Mandate:**
   - Write robust xUnit tests covering business edge cases (school year calculation, threshold triggers, duplicate ingestion).
   - Ensure a **minimum of 85% test coverage** on core business services.

## Acceptance Criteria & Deliverables
- **Functional:** The RESTful endpoints must be fully implemented, following Clean Architecture, and demonstrate resilient idempotency.
- **Developer Experience (DX) & Sample Data:** 
  - Generate a realistic sample JSON batch payload file and save it in `docs/sample_data/attendance_batch_payload.json`.
  - Create an `attendance-api.http` file in the root directory. This file MUST send the `POST /api/attendance/bulk` request by reading the JSON file directly (e.g., using the `< ./docs/sample_data/attendance_batch_payload.json` syntax) to easily test the endpoint directly from the IDE.
- **Validation & Evidence:** The orchestrating agent must execute `dotnet build` and `dotnet test`. It must output terminal evidence of a successful build, the total number of passing tests, and explicit confirmation that the >85% code coverage constraint was met.