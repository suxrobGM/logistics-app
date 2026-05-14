# Phase 9 — Update documentation, rules, and skills

> **Status: PENDING — deferred from 2026-05-14 session** (most updates are Phase-7-driven; reconciliation pass should run after Phase 7 lands). Part of group `03-modular-reorganization/`. Should run after Phase 7 (folders move) so the docs describe the new structure, not the old.

## Goal

Bring every load-bearing doc, rule, skill, and architecture page in line with the new layout. After this phase, neither a human reading docs nor Claude Code reading rules should be misled by stale Phase-0 facts (e.g., "`Application.Contracts` is the email-only project").

## Why

Stale docs silently steer wrong work. The refactor changed enough load-bearing facts (project rename, marker interfaces, transaction behaviour, module layout, etc.) that out-of-date docs become anti-help: they'll route a new contributor (or a Claude Code session) to the old paths.

The doc-update rule across the whole refactor: **no phase PR is mergeable until the docs it invalidates are updated in the same PR.** Phase 9 is the **final reconciliation pass** that catches anything that slipped through, plus the three new docs that didn't exist before.

## Prerequisites

- Phases 1, 2, 3, 4, 5, 6, 7 done (or sufficiently along — some doc updates can be done incrementally; Phase 9 closes anything stale at the end).
- Phase 8 not required — Phase 8 is opportunistic and may run after Phase 9.

## File-by-file update map

| File                                                                                                | Trigger phase(s) | What to update                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| --------------------------------------------------------------------------------------------------- | ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [CLAUDE.md](../../../../CLAUDE.md)                                                                  | 1, 4, 6, 7       | "Architecture (first-pass facts)" section. Current text says _"Application references only interfaces; implementations in `src/Infrastructure/{module}/`"_. Replace with: _"Application references `Application.Abstractions` for infra ports. Application workflow services (`ILoadService`, `IPayrollService`, `IDispatchEligibilityService`, etc.) stay in Application. Infrastructure projects depend on `Application.Abstractions` only."_ Add a one-liner pointing at `Logistics.Architecture.Tests` as the enforcement mechanism. |
| [.claude/feature-map.md](../../../feature-map.md)                                                   | 7                | Every row's "Application path" column changes (`Commands/Load/*` → `Modules/Operations/Loads/Commands/*`). Mechanical rewrite.                                                                                                                                                                                                                                                                                                                                                                                                           |
| [.claude/rules/backend/csharp-conventions.md](../../../rules/backend/csharp-conventions.md)         | 6, 7             | Naming section: commands implement `ICommand`/`ICommand<T>`, queries implement `IQuery<T>`. `IAppRequest` no longer exists. Note that handlers no longer call `SaveChangesAsync` — wrap in `[NoAutoTransaction]` for opt-out.                                                                                                                                                                                                                                                                                                            |
| [.claude/rules/backend/security.md](../../../rules/backend/security.md)                             | 6                | Add a note: validation and feature checks run on queries too, not just commands.                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| [.claude/rules/backend/api-design.md](../../../rules/backend/api-design.md)                         | 1, 6             | If it references service interface locations, point at `Application.Abstractions`.                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| [.claude/rules/backend/ai-agent.md](../../../rules/backend/ai-agent.md)                             | 2                | If it references `IDispatchEligibilityService` for the cross-layer call, point at `IEligibilityCheck`.                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| [docs/architecture/overview.md](../../../../docs/architecture/overview.md)                          | 1, 4, 5, 6, 7    | Biggest update (337 lines today). Refresh the project graph (show `Application.Abstractions` as a sibling of Application). Add a section on the command/query pipeline split. Add a section on architecture tests and what they enforce. Update Registrar example to per-module style.                                                                                                                                                                                                                                                   |
| [docs/architecture/domain-model.md](../../../../docs/architecture/domain-model.md)                  | 2, 3             | If it shows `DispatchEligibilityService` or `.Include()` examples, fix.                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| [docs/architecture/multi-tenancy.md](../../../../docs/architecture/multi-tenancy.md)                | 6                | Add `TransactionBehaviour` semantics: tenant commands auto-transactional, cross-DB commands opt out via `[NoAutoTransaction]`, no 2PC.                                                                                                                                                                                                                                                                                                                                                                                                   |
| [.claude/skills/scaffold-feature/SKILL.md](../../../skills/scaffold-feature/SKILL.md)               | 1, 6, 7          | The biggest skill update. Scaffolding output changes: handler implements `ICommand` (not `IAppRequest`); no `SaveChangesAsync` in handler body; port interfaces go to Abstractions; files placed under `Modules/{X}/`.                                                                                                                                                                                                                                                                                                                   |
| [.claude/skills/add-dispatch-tool/SKILL.md](../../../skills/add-dispatch-tool/SKILL.md)             | 2                | If steps reference `IDispatchEligibilityService` or eligibility DTOs, route to the new locations.                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| [.claude/skills/add-webhook-handler/SKILL.md](../../../skills/add-webhook-handler/SKILL.md)         | 6                | Webhook handlers should be flagged `[NoAutoTransaction]` by default with justification — bake into the skill checklist.                                                                                                                                                                                                                                                                                                                                                                                                                  |
| [.claude/skills/add-llm-provider/SKILL.md](../../../skills/add-llm-provider/SKILL.md)               | 1                | Provider interfaces go to `Application.Abstractions/AiDispatch/`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| [.claude/skills/add-tenant-feature-flag/SKILL.md](../../../skills/add-tenant-feature-flag/SKILL.md) | 6                | If the skill gives a command/handler template, update to `ICommand` + no manual `SaveChangesAsync`.                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| [.claude/skills/migration-creator/SKILL.md](../../../skills/migration-creator/SKILL.md)             | (probably none)  | Verify unaffected (migrations target `Infrastructure.Persistence`, which is unchanged at the migration level).                                                                                                                                                                                                                                                                                                                                                                                                                           |
| [.claude/skills/refactor-and-split/SKILL.md](../../../skills/refactor-and-split/SKILL.md)           | 7                | If it references file paths or module boundaries, update.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |

## New docs to create

### 1. `docs/architecture/layering.md`

Single canonical page describing:

- The 4-layer rule: `Domain ← Abstractions ← Application/Infrastructure ← Presentation`
- The Infrastructure-port vs Application-workflow distinction (the call Codex caught — what stays in Application vs what moves to Abstractions)
- Where each kind of code lives, with one example per layer
- Link to `Logistics.Architecture.Tests` as the enforcement mechanism

### 2. `docs/architecture/cqrs-pipeline.md`

- `ICommand` / `IQuery` markers
- Pipeline behaviour order (Logging → UnhandledException → Validation → FeatureCheck → Transaction)
- `TransactionBehaviour` semantics, including the `Result.Fail` rollback rule
- `[NoAutoTransaction]` opt-out: when and why
- `IMasterCommand` and `ICrossDatabaseCommand` (no 2PC guarantee)
- Replaces ad-hoc references currently scattered in `csharp-conventions.md`

### 3. `docs/architecture/module-layout.md`

- Document the post-Phase-7 layout: `Modules/{Operations, Compliance, Financial, IdentityAccess, Integrations, Platform}/`
- Rules for placing a new feature:
  - Find the right module by bounded context (not by entity)
  - Folder layout within a module: `Commands/`, `Queries/`, `Events/`, optional `Policies/`, `ReadModels/`
  - Module registrar location and what goes in it
- Reference the `scaffold-feature` skill for new-feature scaffolding

## Process per upstream phase

- Open the docs listed for that phase **before** opening the code PR.
- Update docs alongside code so a reviewer sees both in the same diff.
- For skills with templated code blocks, **run the skill once locally** after the update to verify the new scaffolding compiles against the post-refactor architecture.

## Verification

Run at the end of Phase 9 to catch any leftover stale facts:

```bash
# stale Phase-0 terms anywhere in docs/rules/skills
grep -rn "IAppRequest\\|SaveChangesAsync\\|Application/Services\\|Application\\.Contracts" .claude/ docs/ CLAUDE.md
```

Should return zero.

Pick one skill (`scaffold-feature` — highest-value) and **dry-run it** to scaffold a throwaway feature. Confirm the generated code compiles and the new architecture tests pass.

## Risks

| Risk                                                                             | Mitigation                                                                                    |
| -------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `feature-map.md` rewrite breaks links                                            | Click 3-5 links after the rewrite. Spot-check.                                                |
| Skill templates have ASCII art / specific line numbers that refactor invalidated | Read each skill end-to-end; don't trust grep alone.                                           |
| New docs duplicate `csharp-conventions.md`                                       | After writing `cqrs-pipeline.md`, trim `csharp-conventions.md` of anything now covered there. |
| Docs change in a phase PR but the linked file path was renamed in the same PR    | Verify links by clicking.                                                                     |

## Acceptance

- [ ] Stale-term `grep` returns zero.
- [ ] `CLAUDE.md` accurately describes the new layering.
- [ ] `feature-map.md` reflects the new module layout.
- [ ] All 4 backend rule files updated where relevant.
- [ ] All 3 architecture docs (`overview.md`, `domain-model.md`, `multi-tenancy.md`) refreshed.
- [ ] 3 new docs created: `layering.md`, `cqrs-pipeline.md`, `module-layout.md`.
- [ ] All 7 affected skills updated.
- [ ] `scaffold-feature` dry-run produces working code.
- [ ] One or two commits — docs-only changes (no code).
