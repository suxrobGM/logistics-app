# Application Layer Refactor — Plan Index

This folder contains the per-session execution plans for the remaining work on the Application layer refactor.

The **comprehensive master plan** that motivates everything here lives outside the repo at:
`C:\Users\admin\.claude\plans\okay-let-s-build-a-rippling-hollerith.md`

## Current state (as of 2026-05-13)

Branch `refactor/application-abstractions` is **14 code commits ahead of `main`**, all building cleanly with all tests passing. Slices 1.0–1.7 (relocation), Slice 1.9-partial (csproj refs for 6 clean Infrastructure projects), and Slice 1.9-remainder (scoped — 4 of 5 remaining Infrastructure projects) are landed. **Only `Logistics.Infrastructure.AI` still references `Logistics.Application`** — its decoupling is deferred to a dedicated follow-up slice.

```
135f4de6  refactor(app): extract Stripe address mapper to Infrastructure.Payments; switch Payments csproj    ← slice 1.9-remainder PR 5/5
50b27390  refactor(app): introduce ITruckGeolocationUpdater port; decouple Infrastructure.Communications     ← slice 1.9-remainder PR 4/5
f71c2658  refactor(app): switch Infrastructure.Documents to reference Abstractions                           ← slice 1.9-remainder PR 3/5
63c8d552  refactor(app): switch Infrastructure.Persistence to reference Abstractions                         ← slice 1.9-remainder PR 2/5
e09ffeca  refactor(app): move IDataAnonymizer + IDataExportService to Abstractions/Privacy/                  ← slice 1.9-remainder PR 1/5
00ddc2a1  refactor(app): switch 6 Infrastructure projects to reference Abstractions                          ← slice 1.9-partial
9d0cbe7a  refactor(app): move Stripe ports to Abstractions (pure relocation)
4448aa94  refactor(app): move Tax ports and boundary DTOs to Abstractions
979af2c2  refactor(app): move AiDispatch ports to Abstractions
718889ee  refactor(app): move Eld and LoadBoard provider ports to Abstractions
21b12cc9  refactor(app): move Routing and Documents ports to Abstractions
0b0c5891  refactor(app): move tenancy/feature/notification/realtime ports to Abstractions
44a352a1  refactor(app): move Storage/Geocoding/Vin/Captcha ports to Abstractions
977a44f6  refactor(app): rename Application.Contracts to Application.Abstractions
```

### User decisions taken during execution

- **Slice 1.8 (Stripe SDK → DTOs) — DEFERRED.** User chose not to pursue Stripe model decoupling: judged risky, low benefit relative to the cost. `Abstractions.csproj` keeps the `Stripe.net` package reference. The 8 Stripe port interfaces still expose SDK types in their signatures.
- **Abstractions → Domain decoupling — DEFERRED.** Heavy overlap with Slice 1.8 on the Stripe ports. Re-open only after the Stripe direction is revisited. `Abstractions.csproj` keeps the `Logistics.Domain` project reference.
- **Slice 1.9-AI — DEFERRED** to its own follow-up. User chose to land the "easy 4" (Persistence, Documents, Communications, Payments) and defer Infrastructure.AI because every path (move tools to Application vs. introduce `IMediatorAdapter`) has design implications worth their own review.
- **Partial Phase 2 work landed early:** `StripeObjectMapper.ToStripeAddressOptions` was split out and moved to `Infrastructure.Payments/Stripe/StripeAddressMapper.cs` as part of slice 1.9-remainder PR 5, since it was needed to switch Payments to Abstractions cleanly. The other StripeObjectMapper helpers (status mappers, from-Stripe address mapper) remain in Application — they're consumed by Application handlers and a Presentation seeder, so they don't impede the Infrastructure layering.

## Order of work

| Group                           | When  | Status                                                             | Why this order                                                                                       |
| ------------------------------- | ----- | ------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| `01-phase1-completion/`         | First | **PARTIAL** — 1.9-remainder (scoped) landed; 1.8 + Domain deferred | Closes the contract-layer work. Without it, layering is half-done. Arch tests in group 2 would fail. |
| `02-architectural-foundations/` | Next  | Pending — see note on Phase 5 arch-test allow-list below           | Layering hygiene + CI safety net. Protects everything downstream from drift.                         |
| `03-modular-reorganization/`    | Last  | Pending                                                            | Larger reorg + docs. Only valuable after foundations are solid.                                      |

## Folder contents

### [01-phase1-completion/](01-phase1-completion/)

Close out Phase 1.

- `01-slice-1.8-stripe-dto-replacement.md` — **DEFERRED.** Replace Stripe SDK types in port signatures with DTOs; drop `Stripe.net` from Abstractions. User decided risk/benefit didn't justify it in this round.
- `02-slice-1.9-remainder-coupled-infrastructure.md` — **PARTIAL — 4 of 5 projects landed.** Persistence, Documents, Communications, Payments all reference `Abstractions` only as of commits `e09ffeca`..`135f4de6`. **`Infrastructure.AI` remains** and gets its own follow-up plan (`04-slice-1.9-ai-decoupling.md` — TBD).
- `03-abstractions-domain-decoupling.md` — **DEFERRED.** Removing `Logistics.Domain` reference from Abstractions overlaps heavily with Slice 1.8 on the Stripe ports. Re-open after Stripe decision is revisited.

### Follow-up plans not yet written

- **`01-phase1-completion/04-slice-1.9-ai-decoupling.md`** — Decouple `Infrastructure.AI` from Application. Two real options to choose between when this slice opens: (A) move the 6 AI tool classes into `Application/Modules/Integrations/AiDispatch/Tools/`; (B) accept the coupling explicitly and add an allow-list entry to the Phase 5 arch test. Hybrid `IMediatorAdapter` was considered but doesn't sever the csproj reference on its own because tools still construct `Application.Commands.*` types.

### Phase 5 arch-test note

When Phase 5 lands its NetArchTest rules, the `Each_Infrastructure_assembly_references_Abstractions_not_Application` rule must include an allow-list entry for `Logistics.Infrastructure.AI` until slice 1.9-AI resolves it. Remove the allow-list as part of that slice.

### [02-architectural-foundations/](02-architectural-foundations/)

Layering hygiene + CI gate.

- `01-phase2-misplaced-services-and-eligibility-port.md` — Move `StripeObjectMapper` + `TaxCalculationMappings` to Infrastructure; add `IEligibilityCheck` narrow port.
- `02-phase3-aspnet-http-and-include-leaks.md` — Fix `IHttpContextAccessor` in `ImpersonateUserHandler`; remove `.Include()` violation in `AcceptInvitationHandler`.
- `03-phase5-architecture-tests.md` — Add `Logistics.Architecture.Tests` (NetArchTest) as CI gate.
- `04-phase6-command-query-pipeline-transactions.md` — `ICommand`/`IQuery` markers; split pipeline; `TransactionBehaviour`; remove 191 scattered `SaveChangesAsync` calls.

### [03-modular-reorganization/](03-modular-reorganization/)

Internal Application reorg + reads + docs.

- `01-phase4-module-registrars-and-scrutor.md` — Per-module registrars; shrink `Registrar.cs` to orchestration.
- `02-phase7-internal-module-folders.md` — Reorganize `Commands/`/`Queries/` into `Modules/{Operations, Compliance, Financial, IdentityAccess, Integrations, Platform}/`.
- `03-phase8-read-side-projections.md` — Per-module read services for heavy query handlers.
- `04-phase9-docs-rules-skills-update.md` — Update CLAUDE.md, feature-map, rules, skills, architecture docs.
- `05-phase10-optional-project-carveout.md` — Promote stable modules to standalone csproj.

## Rules that apply to every plan

- **Branch from the latest `refactor/application-abstractions`** unless the plan explicitly says rebase onto `main` first.
- **One slice per PR** unless a plan explicitly groups them.
- **Build green at every commit** (`dotnet build Logistics.slnx`).
- **Never `git add src`** — sweep risk. `environment.ts` has already leaked once via this. Stage explicit project paths only.
- **Skip `.mcp.json`, `environment.ts`, anything under `src/Client/`** unless the slice explicitly touches them.
- Each plan ends with an **Acceptance** checklist. Don't open a PR until every item passes.
