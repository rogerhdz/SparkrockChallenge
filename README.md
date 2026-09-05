# Sparkrock Challenge: Attendance Module Migration

## 1. Key Architectural Decisions & Why

* **Clean Architecture:** We structured the solution into `Domain`, `Application`, `Infrastructure`, and `Api` layers. 
  * *Why:* To completely decouple the core business rules from external frameworks (like HTTP or Entity Framework). This guarantees long-term maintainability and makes unit testing the business logic straightforward.
* **Idempotent Ingestion Endpoint:** The `POST /api/attendance/bulk` endpoint is designed to process batches safely, even upon retries. 
  * *Why:* In distributed systems, network drops happen. By implementing a "last-entry-wins" deduplication logic, we ensure that resubmitting the same payload will not corrupt the database or duplicate records.
* **In-Memory Persistence:** We implemented the Repository Pattern backed by EF Core InMemory. 
  * *Why:* To allow reviewers to clone, build, and test the API immediately without needing to configure a local SQL Server, while keeping the data access layer ready to be swapped for a real SQL provider.
* **Agent-Agnostic Workflow (SDD):** We used Spec-Driven Development. We used GitHub Copilot for execution due to active licensing, but provided a `CLAUDE.md` file. 
  * *Why:* To demonstrate that the architecture and workflow dictate the AI's behavior, making the solution portable across different LLMs.

## 2. Handling Legacy Code Ambiguities

During the VB6/SQL to .NET 8 migration, we encountered several ambiguities in the legacy code and handled them as follows:

* **Missing SQL Schema & Stored Procedure Details:** The legacy VB6 code relied heavily on database interactions where the exact schema and stored procedure logic were not fully visible. 
  * *How we handled it:* Instead of guessing the schema, we abstracted the data access using the Repository Pattern. We reverse-engineered the necessary domain entities (`studentId`, `attendDate`, `attendanceCode`) based on their usage in the application logic, and used an In-Memory DB to validate the behavior independently of the legacy schema.
* **Implicit Batch Deduplication Rules:** The legacy loops were not explicit about what happens if a batch contains duplicate records for the same student on the same day. 
  * *How we handled it:* We removed the ambiguity by explicitly defining a "last-entry-wins" contract in the payload processing. We codified this assumption into the Application layer to ensure predictable state.
* **Chronic Absence Alert Nuances:** The exact state transitions for alerts (e.g., what happens when an alert is resolved but the student misses school again) were implicit or scattered. 
  * *How we handled it:* We centralized this logic into a Business Rules Engine within the Domain layer. We then wrote comprehensive `xUnit` tests (achieving 87.25% coverage) to lock in this behavior. The tests now serve as the missing "living documentation" for these edge cases.

## 3. How to Run & Test
1. Run the API: `dotnet run --project src/Attendance.Api/Attendance.Api.csproj`
2. Execute the `attendance-api.http` file at the root using the VS Code REST Client.