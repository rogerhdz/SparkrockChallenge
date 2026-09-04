# K-12 Attendance Module - Architecture & Standards

## 1. Macro Architecture & Solution Layout
The solution must strictly follow **Clean Architecture** principles and be organized under the repository root as follows:

* **`src/`**: Production code layers
  * `src/Attendance.Domain`: Core business logic, entities, and domain exceptions (zero framework dependencies).
  * `src/Attendance.Application`: Use cases, CQRS handlers, DTOs, and business interfaces.
  * `src/Attendance.Infrastructure`: EF Core `InMemory` implementation, DbContext, and persistence concerns.
  * `src/Attendance.Api`: ASP.NET Core 8 Web API, controllers, and dependency injection wiring.
* **`tests/`**: Automated test suites
  * `tests/Attendance.UnitTests`: xUnit project targeting at least 85% coverage on application business rules.
* **Root level restrictions:** No source code or `.sln` files are allowed directly in the root. Only configuration, documentation (`docs/`, `openspec/`), and legacy artifacts (`legacy_code/`).

## 2. Technical Standards
* **Framework:** .NET 8 Web API.
* **Design Patterns:** Logical CQRS separation (Commands for bulk ingestion, Queries for history/status).
* **Dependency Injection:** Scoped lifetime for Entity Framework `DbContext` and application services.
* **Controllers:** Must remain ultra-thin; HTTP concerns only, delegating all logic to application services.

## 3. Testing Standards & Quality Gates
- **Framework & Tooling:** You MUST write comprehensive tests using **xUnit**, **Moq** (or NSubstitute), and ASP.NET Core **`WebApplicationFactory`** for API integration tests.
- **Coverage:** Aim for at least **85% code coverage** on any new or modified application and domain business logic.
- **Isolation & Mocking:** Completely isolate unit tests by mocking external boundaries and dependencies. Utilize EF Core's **InMemory** provider for data persistence tests. Never attempt real network or external infrastructure calls in unit tests.
- **Behavioral Focus:** Test edge cases, validation failures (e.g., invalid school year ranges, duplicate batch keys, 400/404 handling), and idempotency constraints, not just the happy path.