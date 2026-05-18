# Application Layer Refactor — Plan Index

Per-session execution plans for the Application layer refactor. **Each plan file carries its own status header** — this index is just a map.

Master plan (motivation): `C:\Users\admin\.claude\plans\okay-let-s-build-a-rippling-hollerith.md`.

## Status (2026-05-18)

Branch `refactor/application-abstractions` is **45 commits ahead of `main`**, all green.

| Group                                                            | Status                                                                                                                                                                          |
| ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`01-phase1-completion/`](01-phase1-completion/)                 | **Partial.** 1.9-remainder (4 of 5) landed; 1.8, Domain decoupling, and 1.9-AI deferred                                                                                         |
| [`02-architectural-foundations/`](02-architectural-foundations/) | **Done** (Phases 2/3/5). Phase 6 partial: markers + pipeline split landed, `TransactionBehaviour` reverted after audit; see plan                                                |
| [`03-modular-reorganization/`](03-modular-reorganization/)       | **Mostly done.** Phases 4, 7, 9 (done-only scope) landed; Phase 8 partial (read-side projections — one handler split, rest deferred); **Phase 10 skipped** (not worth the cost) |

`Logistics.Infrastructure.AI` is the only Infrastructure project still referencing `Logistics.Application`.

## Active deferrals (their effects in the codebase)

- **Slice 1.8 (Stripe SDK→DTOs).** `Stripe.net` stays in both `Application.csproj` and `Abstractions.csproj`. Phase 5 ships 4 arch assertions as `[Fact(Skip = "Re-enable when slice 1.8 lands")]` placeholders — they activate automatically when 1.8 ships.
- **Slice 1.9-AI.** `Infrastructure.AI` is omitted from the Phase 5 layering rules (`[Theory] InlineData` skips it in `BoundaryTests.Each_Infrastructure_assembly_references_Abstractions_not_Application` and `CsprojReferenceTests.Each_Infrastructure_csproj_does_not_reference_Application_project`) with TODO comments. Drop the omission when 1.9-AI lands.
- **Abstractions → Domain decoupling.** `Abstractions.csproj` keeps the `Logistics.Domain` reference. Re-open after 1.8 is revisited.
- **Phase 9b (docs follow-up).** Phase 9 landed in **done-only scope** — it updated docs for Phases 1 and 7 only. Wording that depends on unshipped work (Phase 2 `IEligibilityCheck` rename, Phase 6 `TransactionBehaviour` / `[NoAutoTransaction]`, and the planned `docs/architecture/cqrs-pipeline.md`) is parked. Reopen Phase 9 when Phases 2 and 6 actually ship.

## Out of scope

- **Phase 10 — Optional project carve-out.** Splitting `Logistics.Application` into multiple csproj projects (one per module). **Skipped — not worth the cost right now.** The modular folder layout from Phase 7 already gives us the boundary clarity we needed; the assembly split would add csproj fan-out, slow incremental build, and complicate Scrutor/MediatR scans for very little additional enforcement that ArchUnitNET tests don't already provide. Revisit only if a concrete pain point (build time, accidental cross-module deps the arch tests miss) emerges.

## Follow-up plans not yet written

- **`01-phase1-completion/04-slice-1.9-ai-decoupling.md`** — Decouple `Infrastructure.AI` from Application. Two real options when this opens: (A) move the 6 AI tool classes into `Application/Modules/Integrations/AiDispatch/Tools/`; (B) accept the coupling explicitly and keep the Phase 5 allow-list. `IMediatorAdapter` doesn't sever the csproj reference because tools still construct `Application.Commands.*` types.

## Rules that apply to every plan

- **Branch from latest `refactor/application-abstractions`** unless the plan says otherwise.
- **One slice per PR**, build green at every commit (`dotnet build Logistics.slnx`).
- **Never `git add src`** — sweep risk; `environment.ts` has leaked once via this. Stage explicit project paths only.
- **Skip `.mcp.json`, `environment.ts`, anything under `src/Client/`** unless the slice explicitly touches them.
- Don't open a PR until the plan's Acceptance checklist passes.
