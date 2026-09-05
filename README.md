# Sparkrock Technical Challenge: Attendance Module Migration

## 📌 Overview
This repository contains the solution for the technical challenge to migrate a legacy K-12 attendance module (VB6/SQL) to a modern **.NET 8** REST API. The solution strictly adheres to **Clean Architecture** principles and a **Spec-Driven Development (SDD)** workflow.

## 🤖 Agent-Agnostic SDD & Tooling Selection
The development of this solution was orchestrated using SDD through OpenSpec artifacts. A core philosophy of this approach is that the workflow is **completely agent-agnostic**.

**Why GitHub Copilot instead of Claude?**
For the execution of this challenge, GitHub Copilot (Workspace/Edits) was utilized primarily due to active licensing and subscription availability. However, the architecture and prompt engineering are not tied to Copilot. 

To demonstrate this, a draft **`CLAUDE.md`** file has been intentionally left in the repository root. This file mirrors the exact same strict state-machine instructions, Clean Architecture constraints, and GitFlow rules used by Copilot. It serves as proof that the exact same predictable, high-quality workflow can be executed using Claude (or any other capable LLM) simply by swapping the agent and feeding it the same SDD context.

## 🏗️ Architecture & Business Logic
* **Clean Architecture:** Strict separation of concerns across layers (`Domain`, `Application`, `Infrastructure`, `Api`).
* **Idempotent Ingestion:** The `POST /api/attendance/bulk` endpoint is designed to safely support retries. It implements a "last entry wins" deduplication logic for batch processing.
* **Business Rules Engine:** Successfully migrated the Chronic Absence Alerts rule. A previously resolved alert does not block the generation of a new alert if the student crosses the threshold again within the same school year.
* **In-Memory Database:** The EF Core InMemory provider was used to ensure the project can be cloned, compiled, and tested immediately without requiring external infrastructure (like a SQL Server instance).

## 🧪 Testing & Validation
* Unit tests implemented using `xUnit` and `FluentAssertions`.
* Business logic validation through infrastructure service mocking.
* **Code coverage exceeds the 85% requirement** (Validated via Cobertura XML, achieving a final result of **87.25%** line coverage).

## 🚧 Identified Tech Debt
During the iterative execution with the AI agent, we identified specific areas of technical debt that were partially mitigated but require further refinement:
1. **Scaffolding Refinement:** By default, AI-generated CLI commands tend to create the solution file (`.sln`), test folders (`tests/`), and test results (`TestResults/`) in the repository root. Time was invested in correcting these paths to respect standard conventions inside the `src/` directory.
2. **Unit Test Definitions:** Although coverage is high, test generation prompts can sometimes be ambiguous. There is a need to standardize naming conventions and mock structures in the initial specs to prevent the AI from assuming incorrect edge-case behaviors.

## 🚀 Future Improvements (Next Steps)
To bring this project up to a fully production-ready enterprise standard, the following improvements are planned:
* **CI/CD Pipeline:** Add GitHub Actions workflows to automatically block PRs if the build fails or if test coverage drops below 85%.
* **Architecture Context Enhancement (`context_architecture.md`):** Define stricter directives regarding dependency injection and layer boundaries to prevent the AI from inadvertently coupling the Application layer with Infrastructure.
* **Official .NET AI Skills:** Integrate framework-specific skills into the AI agent so it natively uses Microsoft's correct templates and scaffolding conventions, reducing initial friction.

## ⚙️ How to Run and Test
1. Clone the repository and open a terminal at the root.
2. Run the API project:
   `dotnet run --project src/Attendance.Api/Attendance.Api.csproj`
3. Open the `attendance-api.http` file included in the root directory.
4. Use the **REST Client** extension (in VS Code) to trigger the Health Check or the Bulk Attendance Ingestion with a single click.

---
*Developed as part of the Sparkrock Technical Assessment.*