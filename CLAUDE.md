# Claude Agent Instructions (K-12 Attendance Module)

## 1. Scope & Ground Truth Context
These instructions apply to the legacy K-12 VB6/SQL migration to .NET 8. We strictly follow the **OpenSpec CLI** framework for Spec-Driven Development (SDD).

Before proposing architecture or writing code, you MUST consult and adhere to the architectural context in:
- `docs/sdd/context_architecture.md` (Defines Clean Architecture layout and technical rules)
- `docs/sdd/context_business.md`
- `docs/sdd/context_legacy_inferences.md`

## 2. Strict Workspace & Solution Layout
When scaffolding the .NET 8 solution, you MUST strictly adhere to the following directory structure under the repository root:
- **`src/`**: Contains all production projects structured in Clean Architecture:
  - `src/Attendance.Domain` (Class Library)
  - `src/Attendance.Application` (Class Library referencing Domain)
  - `src/Attendance.Infrastructure` (Class Library referencing Application/Domain)
  - `src/Attendance.Api` (Web API referencing Application/Infrastructure)
- **`tests/`**: Contains all test projects:
  - `tests/Attendance.UnitTests` (xUnit project referencing Application and Domain)
- **Root level allowed items ONLY:** `.gitignore`, `CLAUDE.md`, `README.md`, `docs/`, `src/`, `tests/`, and `legacy_code/`. Do NOT place source code, `.sln` files, or `TestResults/` directly in the root.

## 3. The Agentic SDD Lifecycle (STRICT STATE MACHINE)
You must act as a strict state machine during the OpenSpec lifecycle. All OpenSpec terminal commands MUST be prefixed with `npx @fission-ai/openspec` (e.g., `npx @fission-ai/openspec status`). Never skip phases or assume approval.

When a user asks to "analyze a requirement", "propose a change for requirement", or mentions a migration file, follow these exact phases:

**Phase 1: Ingestion, Gap Analysis & Drift Detection**
1. **Read & Compare:** Read the active requirement (e.g., `docs/requirements/migration_daily_attendance_requirement.md`), the relevant legacy artifacts in `/legacy_code`, and all Ground Truth context files in `docs/sdd/`.
2. **Perform Gap Analysis:** Cross-reference the requirement against the legacy code reality and the context files. Explicitly look for:
   - Discrepancies or contradictions between the requirement and existing context rules.
   - Missing domain definitions, database assumptions, or edge cases.
3. **Report & Pause:** Output a structured report detailing:
   - **Domain Understanding:** A brief summary of what the requirement aims to achieve.
   - **Discrepancies / Gaps Detected:** Any friction or missing pieces found (or state explicitly if everything is fully aligned).
4. **STOP IMMEDIATELY:** Ask the user: *"Are there context adjustments needed based on these findings, or shall we proceed?"* Wait for either context updates or the exact command `/propose-design`.

**Phase 2: Architectural Proposal (Mandatory Pause)**
5. **Scaffold Change:** Execute `npx @fission-ai/openspec new change <kebab-case-description>` (e.g., `migration-daily-attendance`). The ENTIRE string MUST be in strict lowercase `kebab-case` to avoid CLI errors.
6. **Generate:** Create `proposal.md` and `design.md` based strictly on the requirement description and the Clean Architecture rules.
7. **STOP IMMEDIATELY:** Ask the user to review the design. Do NOT generate specs or tasks yet. Wait for the exact command `/approve-design`.

**Phase 3: Contract & Tasks (Second Mandatory Pause)**
8. **Generate:** ONLY when the user inputs `/approve-design`, proceed to generate `spec.md` and `tasks.md`.
9. **STOP IMMEDIATELY:** Ask the user to review the implementation steps in `tasks.md`. Do NOT create branches or write any code yet. Wait for the exact command `/approve-tasks`.

**Phase 4: Pre-Implementation Branching (GitOps)**
10. **Isolate Work:** ONLY after `/approve-tasks`, execute safely in the terminal:
   - `git add .`
   - `git stash`
   - `git checkout develop || git checkout -b develop`
   - `git pull origin develop || true`
   - `git checkout -b <type>/<lowercase-JIRA-ID>-<short-name>`
   - `git stash pop || true`
   Wait for terminal confirmation that the branch was successfully created and active.

**Phase 5: Implementation, Validation & Evidence**
11. **Code & Finalize:** Implement the tasks. You MUST strictly respect the `src/` and `tests/` folder segregation defined in Section 2.
12. **Testing:** Write robust xUnit tests targeting at least **85% code coverage**.
13. **Execution & Evidence:** Execute `dotnet build src/<SolutionName>.sln` and `dotnet test src/<SolutionName>.sln --collect:"XPlat Code Coverage"`. You MUST parse the terminal output and provide a final summary report in the chat containing:
    - Build status (Success/Failure).
    - Total tests executed, passed, and failed.
    - Final code coverage percentage.