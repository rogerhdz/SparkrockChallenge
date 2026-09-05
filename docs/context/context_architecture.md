# Macro-Architecture & Constraints Context

This document defines the strict architectural guardrails for the .NET 8 migration. The AI agent MUST adhere to these rules during the proposal, design, and implementation phases.

## 1. Clean Architecture Strict Separation
- The solution MUST follow Clean Architecture principles.
- **Layers:** 
  - `Domain`: Core entities, Enums, and Domain Exceptions. No external dependencies.
  - `Application`: CQRS Handlers, Services, Interfaces, and DTOs.
  - `Infrastructure`: EF Core DbContext, Data Models, Repositories (if any), and External Services.
  - `API`: Ultra-thin ASP.NET Core Web API controllers/minimal APIs.
- **Dependency Rule:** API -> Infrastructure & Application -> Domain. The Domain layer must have zero dependencies on other projects.

## 2. Dependency & Project Wiring Mandate
- **Explicit Project References:** When scaffolding the Clean Architecture solution, you MUST explicitly wire the `.csproj` files using `dotnet add reference` (e.g., API references Infrastructure & Application; Infrastructure references Application; Application references Domain) before writing any C# implementation.
- **Explicit Package References:** You MUST install required NuGet packages (e.g., `Microsoft.EntityFrameworkCore.InMemory`, `xunit`, `Moq`) into their respective `.csproj` files via CLI commands before implementing the classes that depend on them.
- **Iterative Validation:** After generating project scaffolding or altering dependencies, you MUST run `dotnet restore` and `dotnet build` to ensure a healthy dependency graph before proceeding to the business logic implementation.

## 3. Program.cs & Top-Level Statements Mandate
- **Top-Level Statements Strictness:** The `.NET 8` API template uses top-level statements by default in `Program.cs`. You MUST NOT introduce `namespace` or `class` declarations (such as `public class Program`) into this file. 
- **Code Order:** All Dependency Injection (DI) registrations, middleware configurations, and routing MUST be written cleanly as top-level statements. Any custom types or extensions must be extracted to separate files, NEVER appended to the bottom of `Program.cs`.

## 4. Strongly-Typed Contracts Mandate
- **No Anonymous Types:** The Application layer (Services, CQRS Handlers) MUST NOT return anonymous objects (e.g., `return new { TotalAbsences = 10 };`) or use `dynamic` under any circumstances.
- **Explicit DTOs / Records:** Every query and command response MUST be returned as a strongly-typed C# `class` or `record` (e.g., `ChronicStatusResponseDto`) defined explicitly in the Application layer.
- **Cross-Project Testability:** xUnit tests must assert against these concrete, strongly-typed objects. Relying on `RuntimeBinder` or reflection to evaluate dynamic properties is strictly forbidden.

## 5. Persistence & Testing Strategy
- **Persistence:** Use `Microsoft.EntityFrameworkCore.InMemory` for all data operations. No real SQL Server database is required for this phase.
- **Testing Framework:** Use `xUnit`.
- **Test Coverage Target:** 85% line coverage minimum. The implementation phase is NOT complete until `dotnet test` generates a Cobertura report proving this threshold is met.