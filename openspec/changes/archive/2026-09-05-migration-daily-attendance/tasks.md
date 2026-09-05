## 1. Foundation and Domain Model

- [x] 1.1 Create the .NET 8 Clean Architecture solution structure with Domain, Application, Infrastructure, and API projects and verify the project references are wired correctly
- [x] 1.2 Add EF Core InMemory and xUnit dependencies to the correct projects and verify restore succeeds
- [x] 1.3 Define attendance domain entities and value objects for school, term, attendance record, summary, and alert state and verify the model compiles

## 2. Persistence and Query Contracts

- [x] 2.1 Implement the EF Core DbContext and seed data for schools, terms, attendance codes, and sample records and verify the in-memory database initializes successfully
- [x] 2.2 Implement the attendance repository and query service contracts for history and chronic status and verify the query layer compiles cleanly against the Application layer

## 3. Application Service Logic

- [x] 3.1 Implement the bulk ingestion command service with student-date upsert semantics and verify duplicate batch records resolve to the last entry in the request
- [x] 3.2 Implement the September-based school-year calculation and term resolution logic and verify the business rule for year boundaries passes unit tests
- [x] 3.3 Implement chronic absence counting and threshold evaluation and verify the threshold-triggered alert logic matches the design requirements
- [x] 3.4 Implement alert lifecycle behavior for unresolved-versus-resolved state and verify the system can recreate an alert after resolution when the threshold is crossed again

## 4. API Surface

- [x] 4.1 Add minimal API endpoints for `POST /api/attendance/bulk`, student history lookup, and chronic status evaluation and verify the routes are reachable via integration-level tests
- [x] 4.2 Add request/response DTOs and mapping logic and verify the JSON contract matches the required payload shape without legacy XML naming

## 5. Sample Data and Developer Experience

- [x] 5.1 Create realistic sample attendance payload data under `docs/sample_data/attendance_batch_payload.json` and verify it matches the bulk API contract
- [x] 5.2 Create `attendance-api.http` at the repo root with a `POST /api/attendance/bulk` request that reads the JSON file and verify the file is usable in the IDE

## 6. Validation and Evidence

- [x] 6.1 Write xUnit tests covering duplicate ingestion, school-year boundaries, threshold triggers, and alert reactivation and verify the suite passes
- [x] 6.2 Run `dotnet build` and `dotnet test` and verify the output confirms successful build, passing tests, and coverage above 85%
