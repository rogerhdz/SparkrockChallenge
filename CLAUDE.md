# Backend Agent Instructions (K-12 Attendance Module)

## 1. Scope & Ground Truth Context
These instructions apply to the legacy K-12 VB6/SQL migration to .NET 8. We strictly follow the **OpenSpec CLI** framework for Spec-Driven Development (SDD).
Before proposing architecture or writing code, you MUST consult and adhere to:
- `docs/sdd/context/context_architecture.md`
- `docs/sdd/context/context_business.md`
- `docs/sdd/context/context_legacy_inferences.md`

## 2. Strict Workspace & Solution Layout
When scaffolding the .NET 8 solution, you MUST adhere to the following directory structure under the repository root:
- **`src/`**: Contains all production projects structured in Clean Architecture:
  - `src/Attendance.Domain` (Class Library)
  - `src/Attendance.Application` (Class Library referencing Domain)
  - `src/Attendance.Infrastructure` (Class Library referencing Application/Domain)
  - `src/Attendance.Api` (Web API referencing Application/Infrastructure)
- **`tests/`**: Contains all test projects:
  - `tests/Attendance.UnitTests` (xUnit project referencing Application and Domain)
- **Root level allowed items ONLY:** `CLAUDE.md`, `config.yaml`, `docs/`, `openspec/`, and `legacy_code/`. Do NOT place source code or solution files directly in the root.

## 3. The Agentic SDD Lifecycle (STRICT STATE MACHINE)
if a user ask to "analize a requirement", "propose a change for requirement".
You must act as a strict state machine during the OpenSpec lifecycle. You have access to OpenSpec skills in the `.commandcode/skills` directory. Never skip phases or assume approval.

**Phase 1: Ingestion, Gap Analysis & Drift Detection**
1. **Read & Compare:** Read the active requirement, the relevant legacy artifacts/code in `/legacy_code`, and all Ground Truth context files in `docs/sdd/context`.
2. **Perform Gap Analysis:** Cross-reference the requirement against the legacy code reality and the context files. Explicitly look for:
   - Discrepancies or contradictions between the requirement and existing context rules.
   - Missing domain definitions, database assumptions, or edge cases not covered by the current context.
3. **Report & Pause:** Output a structured report detailing:
   - **Domain Understanding:** A brief summary of what the requirement aims to achieve.
   - **Discrepancies / Gaps Detected:** Any friction or missing pieces found (or state explicitly if everything is fully aligned).
4. **STOP IMMEDIATELY:** Ask the user: *"Are there context adjustments needed based on these findings, or shall we proceed?"* Wait for either context updates or the exact command `/propose-design`.

**Phase 2: Architectural Proposal (Mandatory Pause)**
4. **Generate:** Use the `openspec-propose` skill to create `proposal.md` and `design.md` based strictly on the extracted domain requirements and our Clean Architecture rules.
5. **STOP IMMEDIATELY:** Ask the user to review the design. Do NOT generate specs or tasks yet. Wait for the exact command `/approve-design`.

**Phase 3: Contract & Tasks (Second Mandatory Pause)**
6. **Generate:** ONLY when the user inputs `/approve-design`, use the `openspec-update-change` skill to generate `spec.md` and `tasks.md`.
7. **STOP IMMEDIATELY:** Ask the user to review the implementation steps in `tasks.md`. Do NOT create branches or write any code yet. Wait for the exact command `/approve-tasks`.

**Phase 4: Pre-Implementation Branching (GitOps)**
8. **Isolate Work:** ONLY when the user inputs `/approve-tasks`, you MUST isolate the work in a clean branch. Dynamically infer an appropriate type (`feat`, `fix`, `refactor`, or `chore`) and a short, descriptive kebab-case name based on the active ticket (e.g., `feat/daily-attendance-ingestion`). Execute safely in the terminal:
   - `git add .`
   - `git stash`
   - `git checkout develop || git checkout -b develop`
   - `git pull origin develop || true`
   - `git checkout -b <type>/<lowercase-short-name>`
   - `git stash pop || true`
   - Wait for terminal confirmation that the branch was successfully created and active.

**Phase 5: Implementation & Validation**
9. Write comprehensive xUnit unit tests targeting at least 85% test coverage for the attendance service and business rules.
10. Execute dotnet build and dotnet test /p:CollectCoverage=true (or standard test runner) to verify behavior and ensure all tests pass successfully.