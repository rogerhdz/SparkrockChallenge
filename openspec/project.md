# Legacy K-12 VB6/SQL Migration — OpenSpec Project Overview

This is the normative source of truth for the legacy K-12 VB6/SQL migration capabilities. All OpenSpec changes, specs, and implementation decisions derive from the constraints defined here. For macro-architecture details, refer to `docs/sdd/context/context_architecture.md`.

## Capability Map & Spec Granularity

When creating new features, assign them to one of these capabilities. If no capability fits, create a new prefixed kebab-case capability (`api-*`) and add a row to the map below in the same change.

| Capability | Owns |
|------------|------|
| `api-foundation` | Foundation only: Solution scaffold, EF Core InMemory setup, Clean Architecture layers, Swagger configuration. Do NOT add feature requirements here. |
| `api-attendance` | Core business logic: Bulk attendance ingestion, idempotent upserts, chronic absenteeism calculation, and alert generation. |