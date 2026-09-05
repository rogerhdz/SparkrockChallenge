## Why

The legacy attendance module relies on a VB6 form that serializes student rows into XML and sends them to SQL stored procedures for upserts, alert generation, and summary recalculation. That makes the logic hard to validate, difficult to extend, and tightly coupled to SQL behavior. This change introduces a modern .NET 8 API that preserves the correct business rules while making the attendance workflow testable, resilient, and aligned with Clean Architecture.

## What Changes

- Add a bulk attendance ingestion capability that accepts a standard JSON list of attendance records instead of the legacy XML contract.
- Preserve the legacy business behavior for school-year calculation, term resolution, and chronic absenteeism recalculation.
- Enforce idempotent batch handling, with last-entry-wins semantics when multiple records exist for the same student and date in a single request.
- Expose query endpoints for student attendance history and chronic status evaluation.
- Provide realistic sample payloads and IDE-testable request files for direct validation.
- Establish an xUnit-first validation path with coverage targets and explicit evidence for build and tests.

## Capabilities

### New Capabilities
- `api-attendance`: attendance bulk ingestion, history queries, chronic status evaluation, and alert lifecycle rules for the daily attendance migration.

### Modified Capabilities
- None.

## Impact

- The change affects the legacy attendance workflow and its SQL assumptions, but will be implemented as a new .NET 8 Web API using Clean Architecture boundaries.
- The API will introduce new request/response contracts for bulk attendance submission and query operations without leaking legacy XML naming conventions into the domain model.
- The migration will require new domain, application, infrastructure, and API project structure plus xUnit validation for the business rules and coverage threshold.
