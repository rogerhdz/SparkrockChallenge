# MIGRATION REQUIREMENT: K-12 Legacy VB6/SQL Attendance Module to .NET 8

## Objective
Migrate the legacy desktop attendance module (`frmDailyAttendance.frm` and associated stored procedures) into a modern, resilient .NET 8 Web API following Clean Architecture principles and Spec-Driven Development (SDD).

## Scope of Work
1. **Bulk Ingestion Endpoint (`POST /api/attendance/bulk`):**
   - Must handle batch payloads efficiently (replacing the legacy XML approach).
   - Enforce strict **Idempotency** using upsert patterns via EF Core InMemory.
   - Implement business rules for School Year calculation (September-based) and Term resolution.
   - Recalculate chronic absenteeism dynamically (`COUNT` of absences) and evaluate the threshold (default: 10) to generate alerts idempotently.
2. **Query Endpoints:**
   - History endpoint for student attendance.
   - Chronic status evaluation endpoint.
3. **Quality & Testing Mandate:**
   - Write robust xUnit tests covering business edge cases (school year calculation, threshold triggers, duplicate ingestion).
   - Ensure a **minimum of 85% test coverage** on core business services.