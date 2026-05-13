# Application Layer Refactor — Plan Index

This folder contains the per-session execution plans for the remaining work on the Application layer refactor.

The **comprehensive master plan** that motivates everything here lives outside the repo at:
`C:\Users\admin\.claude\plans\okay-let-s-build-a-rippling-hollerith.md`

## Current state (as of 2026-05-13)

Branch `refactor/application-abstractions` is **9 commits ahead of `main`**, all building cleanly. Slices 1.0–1.7 (relocation) and Slice 1.9 partial (csproj refs for 6 clean Infrastructure projects) are landed.

```
00ddc2a1  refactor(app): switch 6 Infrastructure projects to reference Abstractions
9d0cbe7a  refactor(app): move Stripe ports to Abstractions (pure relocation)
4448aa94  refactor(app): move Tax ports and boundary DTOs to Abstractions
979af2c2  refactor(app): move AiDispatch ports to Abstractions
718889ee  refactor(app): move Eld and LoadBoard provider ports to Abstractions
21b12cc9  refactor(app): move Routing and Documents ports to Abstractions
0b0c5891  refactor(app): move tenancy/feature/notification/realtime ports to Abstractions
44a352a1  refactor(app): move Storage/Geocoding/Vin/Captcha ports to Abstractions
977a44f6  refactor(app): rename Application.Contracts to Application.Abstractions
```

## Order of work

| Group                           | When  | Why this order                                                                                       |
| ------------------------------- | ----- | ---------------------------------------------------------------------------------------------------- |
| `01-phase1-completion/`         | First | Closes the contract-layer work. Without it, layering is half-done. Arch tests in group 2 would fail. |
| `02-architectural-foundations/` | Next  | Layering hygiene + CI safety net. Protects everything downstream from drift.                         |
| `03-modular-reorganization/`    | Last  | Larger reorg + docs. Only valuable after foundations are solid.                                      |

## Folder contents

### [01-phase1-completion/](01-phase1-completion/)

Close out Phase 1.

- `01-slice-1.8-stripe-dto-replacement.md` — Replace Stripe SDK types in port signatures with DTOs; drop `Stripe.net` from Abstractions.
- `02-slice-1.9-remainder-coupled-infrastructure.md` — Resolve the 5 Infrastructure projects still referencing `Application` (AI, Communications, Documents, Payments, Persistence).
- `03-abstractions-domain-decoupling.md` — Remove `Logistics.Domain` reference from Abstractions (pure-ports posture).

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
