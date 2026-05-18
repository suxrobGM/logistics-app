# Slice 1.9 (remainder) — Decouple the 5 Infrastructure projects still referencing Application

> **Status (2026-05-13): PARTIAL — 4 of 5 projects landed.** Persistence, Documents, Communications, and Payments now reference `Logistics.Application.Abstractions` only. `Infrastructure.AI` remains and is deferred to its own follow-up slice (see "Remaining work" below).
>
> Build green, all tests passing (`Logistics.Application.Tests`, `Logistics.Infrastructure.AI.Tests`, `Logistics.Infrastructure.Payments.Tests`, `Logistics.Infrastructure.Persistence.Tests`, `Logistics.Infrastructure.Tax.Tests`, `Logistics.Infrastructure.Vin.Tests` — 453/453).
>
> ## What landed (commits `e09ffeca` → `135f4de6`)
>
> | PR  | Commit     | Project                          | Summary                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
> | --- | ---------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
> | 1   | `e09ffeca` | (port relocation, multi-project) | Moved `IDataAnonymizer` **and** `IDataExportService` from `Application/Services/Privacy/` to `Abstractions/Privacy/`. Audit was wrong to flag only `IDataAnonymizer`; Documents also implements `IDataExportService`, so both ports moved.                                                                                                                                                                                                                                                                                                                         |
> | 2   | `63c8d552` | `Infrastructure.Persistence`     | Moved `IUserService → UserService` DI registration from `PersistenceInfrastructureBuilder.AddMasterDatabase()` to `Logistics.Application.Registrar.AddApplicationLayer()`. Persistence csproj switched to Abstractions. Added explicit `MediatR` and `Logistics.Shared.Identity` refs (previously transitive).                                                                                                                                                                                                                                                     |
> | 3   | `f71c2658` | `Infrastructure.Documents`       | csproj switched from Application to Abstractions. Added explicit `Microsoft.EntityFrameworkCore` ref (previously transitive).                                                                                                                                                                                                                                                                                                                                                                                                                                      |
> | 4   | `50b27390` | `Infrastructure.Communications`  | Introduced narrow port `ITruckGeolocationUpdater` in `Abstractions/Realtime/`; impl in `Application/Services/Tracking/TruckGeolocationUpdater.cs` wraps the existing `SetTruckGeolocationCommand`. `TrackingHub` depends on the port, not `IMediator`. csproj switched. Added explicit `Logistics.Mappings` ref.                                                                                                                                                                                                                                                   |
> |     |            | `Logistics.IdentityServer`       | Side fix: added explicit `Logistics.Application` ProjectReference to IdentityServer — its `Pages/Account/Manage/Profile/Index.cshtml.cs` consumes `IUserService` and was getting Application transitively through Persistence before this slice.                                                                                                                                                                                                                                                                                                                   |
> | 5   | `135f4de6` | `Infrastructure.Payments`        | Split `StripeObjectMapper`: extracted `ToStripeAddressOptions` + `GetCountryCode` into new internal `Infrastructure.Payments/Stripe/StripeAddressMapper.cs`. Other helpers (`GetSubscriptionStatus`, `GetPaymentStatus`, `ToAddressEntity`) stay in `Application/Services/Stripe/StripeObjectMapper.cs` because Application handlers and Presentation seeders use them. csproj switched. Added explicit `Microsoft.Extensions.Caching.Memory` ref to `Infrastructure.Tax` (it consumed `AddMemoryCache()` and got it transitively through Payments → Application). |
>
> ## Final-state audit
>
> ```bash
> grep -rln "Logistics\.Application\.csproj" src/Infrastructure --include='*.csproj'
> # → src/Infrastructure/Logistics.Infrastructure.AI/Logistics.Infrastructure.AI.csproj  (only)
>
> grep -rn '^using Logistics\.Application\.' src/Infrastructure --include='*.cs' | grep -v '\.Abstractions'
> # → 6 hits, all in Infrastructure.AI/Tools/ (the deferred slice)
> ```
>
> ## Notable detours from the originally-approved plan
>
> 1. **PR 1 expanded to two ports.** Audit had flagged only `IDataAnonymizer`; in fact `Infrastructure.Documents/Privacy/DataExportService.cs` implements `IDataExportService` (also in `Application/Services/Privacy/`), so both interfaces moved together.
> 2. **PR 2 needed a DI re-home.** Switching `Infrastructure.Persistence.csproj` to Abstractions only is not enough — `PersistenceInfrastructureBuilder` registers `IUserService` (both interface and impl live in `Application/Services/User/`). The fix was to move that single `services.AddScoped<IUserService, UserService>()` line into `Application/Registrar.AddApplicationLayer()`, then Persistence stops needing the `Logistics.Application.Services` using.
> 3. **Transitive-ref flush.** Switching csprojs to Abstractions exposed several transitive package/project references that had been silently inherited from `Application`. Each affected csproj gained an explicit ref:
>    - `Infrastructure.Persistence` → `MediatR`, `Logistics.Shared.Identity`
>    - `Infrastructure.Documents` → `Microsoft.EntityFrameworkCore`
>    - `Infrastructure.Communications` → `Logistics.Mappings`
>    - `Infrastructure.Tax` → `Microsoft.Extensions.Caching.Memory` (transitively via Payments → Application before)
>    - `Logistics.IdentityServer` → `Logistics.Application` (Profile page consumes `IUserService`)
> 4. **PR 4 used a narrow port instead of `IMediatorAdapter`.** The Communications surface is exactly one MediatR call site; a `ITruckGeolocationUpdater` port models what's actually being done. No generic adapter was introduced.
> 5. **PR 5 partial Phase 2 work.** Splitting `StripeObjectMapper` was the only way to remove Payments' real Application using. The remaining helpers stay in Application until the deferred Stripe slice runs.
>
> ## Remaining work — `Infrastructure.AI`
>
> 6 tool files still import `Application.Commands` / `Application.Queries` / `Application.Services`:
>
> - `Tools/AssignLoadToTruckTool.cs` (`AssignLoadToTruckCommand`)
> - `Tools/CreateTripTool.cs` (`CreateTripCommand`)
> - `Tools/DispatchTripTool.cs` (`DispatchTripCommand`)
> - `Tools/GetUnassignedLoadsTool.cs` (`GetUnassignedLoadsQuery`)
> - `Tools/PreviewTaxCalculationTool.cs` (`PreviewInvoiceTaxQuery`)
> - `Tools/CheckDispatchEligibilityTool.cs` (`IDispatchEligibilityService`)
>
> Open design questions for the follow-up slice (write as `04-slice-1.9-ai-decoupling.md`):
>
> - **Option A — move tools.** Relocate the 6 tool classes into `Application/Modules/Integrations/AiDispatch/Tools/`. `Infrastructure.AI` keeps only LLM provider plumbing. Cleanest long-term; largest diff.
> - **Option B+exception — keep AI's Application ref.** Document Infrastructure.AI as an explicit exception in the Phase 5 arch-test allow-list. The earlier "`IMediatorAdapter`" sketch was reconsidered: hiding MediatR doesn't help because tools still construct `AssignLoadToTruckCommand` etc., so the csproj ref to Application is required regardless.
>
> Section below is the original execution recipe and is retained for context. **Don't re-run sections 1–4 above as written for AI** — they assume only a using-line cleanup, but the AI tools actually need to construct command types.

## Goal

After Slice 1.9-partial (commit `00ddc2a1`), 6 of 11 Infrastructure projects reference `Logistics.Application.Abstractions` only. **5 still reference `Logistics.Application`** because of real residual couplings that are not mere stale usings. This plan resolves each one.

## Why

Until every Infrastructure project depends only on `Abstractions`, the layering goal of Phase 1 is half-done. Phase 5 arch tests will fail with a `Each_Infrastructure_assembly_references_Abstractions_not_Application` rule. More importantly, current state allows Infrastructure code to take new dependencies on application internals without anything to stop it.

## The 5 projects and their couplings

| Project                         | What it imports from Application                                                                    | Nature of the coupling                                                                                                                                                                                                                                               |
| ------------------------------- | --------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Infrastructure.AI`             | `Logistics.Application.Commands`, `Logistics.Application.Queries`, `Logistics.Application.Services` | AI dispatch tools (`AssignLoadToTruckTool`, `CreateTripTool`, `DispatchTripTool`, `GetUnassignedLoadsTool`, `PreviewTaxCalculationTool`, `CheckDispatchEligibilityTool`) **invoke MediatR commands/queries**. Anti-pattern: infra dispatching application use cases. |
| `Infrastructure.Communications` | `Logistics.Application.Commands`                                                                    | Notification handler somewhere dispatches commands. Smaller surface, narrow fix.                                                                                                                                                                                     |
| `Infrastructure.Documents`      | `Logistics.Application.Services.Privacy`                                                            | Implements `IDataAnonymizer` (or similar). Phase 1 master plan kept the **interface** in Application as a "workflow," but the **implementation** is here. Layering inconsistency.                                                                                    |
| `Infrastructure.Payments`       | `Logistics.Application.Services`                                                                    | Uses extension methods on `StripeObjectMapper` (`Address.ToStripeAddressOptions()`, etc.). `StripeObjectMapper` is in Application but is Stripe-shaped.                                                                                                              |
| `Infrastructure.Persistence`    | `Logistics.Application.Services.Privacy`, `Logistics.Application.Services`                          | Implements `IDataAnonymizer`, `ICurrentUserService` impl, `IFeatureService` impl, `ICurrentTenantAccessor` impl, `ITenantDatabaseService` impl, `AiQuotaService` impl, etc. Same workflow/port confusion.                                                            |

## Why each requires real thought (not a mechanical fix)

The Phase 1 plan classified some interfaces as "Application workflows" (stay in Application) and others as "Infrastructure ports" (move to Abstractions). But several interfaces marked "workflow" are actually implemented in Infrastructure projects. That makes them ports — the classification was wrong.

This is a **design decision** per coupling, not a search-and-replace.

## Resolution recipe per coupling

### 1. `Infrastructure.AI` ↔ MediatR Commands/Queries (highest leverage)

**Options:**

- **(A) Move AI tools into Application.** Each `*Tool` class becomes part of an Application module (e.g., `Modules/Integrations/AiDispatch/Tools/`). Infrastructure.AI then only handles LLM provider plumbing (Anthropic, OpenAI HTTP clients).
- **(B) Introduce an `IMediatorAdapter` port.** Define a thin facade in Abstractions: `Task<TResult> SendAsync<TResult>(ICommand<TResult>) ` / `Task<TResult> QueryAsync<TResult>(IQuery<TResult>)`. Implementation in Presentation/Composition root wraps MediatR. Tools take the adapter, not `IMediator` directly. **Hides MediatR from Infrastructure but doesn't move tools.**
- **(C) Accept the coupling explicitly.** Keep the `Application` reference for `Infrastructure.AI`; document it as a known exception. The arch test gets a comment-marked allow-list.

**Recommendation:** (A) is cleanest long-term. (B) is a smaller diff and unblocks Phase 5 arch tests. (C) is honest if neither (A) nor (B) is in this session's budget.

Pick one and execute. Don't try to do all three.

### 2. `Infrastructure.Communications` ↔ `Application.Commands`

Smaller version of the same problem. Identify the one or two files that import `Logistics.Application.Commands` and decide: move the consuming class to Application, introduce the same `IMediatorAdapter`, or accept the coupling.

### 3. `Infrastructure.Documents` ↔ `Application.Services.Privacy`

`Infrastructure.Documents` implements one or more privacy services. The fix:

- **Move the interface(s) into Abstractions** (`Abstractions/Privacy/`). They are ports — implementations are in Infrastructure, so by definition they're not application workflows.
- The Phase 1 plan's "workflow vs port" classification was off here. This is the correction.
- Application keeps any orchestration that wraps the port (e.g., a command handler that calls the port).

### 4. `Infrastructure.Payments` ↔ `StripeObjectMapper` (resolved in Phase 2)

This one already has a plan in Phase 2: move `StripeObjectMapper.cs` from `Application/Services/Stripe/` to `Infrastructure.Payments/Stripe/`. Once moved, the `using Logistics.Application.Services;` line in `StripeCustomerService.cs` and `StripeConnectService.cs` becomes orphan and can be removed.

**You can do this here** in Slice 1.9-remainder, OR defer to Phase 2. Either is fine. If you do it here, note in the commit message that Phase 2 already plans for it.

### 5. `Infrastructure.Persistence` ↔ multiple Application/Services/Privacy + /Services interfaces

The deepest tangle. `Infrastructure.Persistence` implements many "service" interfaces that Phase 1 kept in Application. Per the Slice 1.9 audit, the implementations include:

- `IDataAnonymizer` (privacy)
- `AiQuotaService` (concrete; the interface `IAiQuotaService` is already in Abstractions)
- `FeatureService` (concrete; the interface `IFeatureService` is already in Abstractions)
- `SystemSettingService` (concrete; the interface `ISystemSettingService` is already in Abstractions)
- `CurrentTenantAccessor`, `TenantDatabaseService` (concretes; interfaces already in Abstractions/Tenancy)
- `CurrentUserService` (concrete; interface `ICurrentUserService` already in Abstractions/CurrentUser)
- `NoopCurrentUserService` (test/seeder fallback)

Most of these import `Logistics.Application.Services` simply because the interfaces _used to_ live there. The `using` lines were stripped by Slice 1.9-partial cleanup for files that don't reference any still-in-Application names. The remaining ones probably need:

- `using Logistics.Application.Abstractions.{Privacy,Features,SystemSettings,Tenancy,CurrentUser};` already added in earlier slices
- Inspect each remaining `using Logistics.Application.Services` and verify what it brings in. If only legacy references, drop it.

For `IDataAnonymizer` specifically: move it from `Application/Services/Privacy/` into `Abstractions/Privacy/` (same as recipe #3).

## Prerequisites

- On `refactor/application-abstractions` at or after commit `00ddc2a1` (Slice 1.9-partial).
- Slice 1.8 not required — these are orthogonal.

## Step-by-step

1. **Pick one project, finish it, commit, move on.** Don't try all 5 in a single PR.
2. For each project:
   - `grep -rn "^using Logistics\\.Application\\." src/Infrastructure/{ProjectName} --include='*.cs' | grep -v '\\.Abstractions'` — list every Application coupling.
   - For each unique import line, decide which recipe applies above.
   - Execute the chosen recipe.
   - Update the project's `.csproj` to reference `Application.Abstractions` instead of `Application`.
   - `dotnet build Logistics.slnx` — verify 0 errors.
   - Commit.
3. After all 5 done, verify:
   ```bash
   grep -rln "Logistics\\.Application\\.csproj" src/Infrastructure --include='*.csproj'
   ```
   should return **zero**.

## Suggested order

1. **Infrastructure.Payments** — smallest fix; move `StripeObjectMapper`.
2. **Infrastructure.Documents** — single port move.
3. **Infrastructure.Persistence** — most ports to migrate, but mechanical once decided.
4. **Infrastructure.Communications** — single-file coupling.
5. **Infrastructure.AI** — biggest design call; do last when others are clean.

## Critical files

- All 5 Infrastructure project `.csproj` files
- `src/Core/Logistics.Application/Services/Privacy/IDataAnonymizer.cs` and similar Privacy interfaces (likely moves to Abstractions)
- `src/Core/Logistics.Application/Services/Stripe/StripeObjectMapper.cs` (likely moves to `Infrastructure.Payments/Stripe/StripeObjectMapper.cs`)
- AI tool files under `src/Infrastructure/Logistics.Infrastructure.AI/Tools/` (and possibly the Abstractions surface that supports them)

## Verification

- `dotnet build Logistics.slnx` — 0 errors after each per-project commit.
- `grep -rln "Logistics\\.Application\\.csproj" src/Infrastructure --include='*.csproj'` returns nothing.
- `grep -rln "^using Logistics\\.Application\\." src/Infrastructure --include='*.cs' | grep -v Abstractions` returns nothing.
- Any tests touching AI dispatch tools, privacy data export, Stripe webhooks, tenant resolution still pass.

## Risks

| Risk                                                                                                                         | Mitigation                                                                                            |
| ---------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Choosing option (A) for AI tools triggers massive file moves                                                                 | Time-box; if (A) blows budget, fall back to (B) or (C) explicitly.                                    |
| Moving `IDataAnonymizer` from Application to Abstractions breaks consumers                                                   | Phase 1 pattern already handled this: relocate interface, update namespace, add `using` in consumers. |
| `CurrentUserService` ↔ `IHttpContextAccessor` interaction (Presentation will inject `IHttpContextAccessor` to concrete impl) | Phase 3 has the matching fix. Coordinate.                                                             |

## Acceptance

- [ ] All 5 Infrastructure `.csproj` files reference `Application.Abstractions`, not `Application`.
- [ ] No `using Logistics.Application.X;` (where X is not Abstractions) appears in any `src/Infrastructure/**/*.cs`.
- [ ] If AI tools moved to Application (option A): test suite passes; tool registration in DI still works.
- [ ] If `IMediatorAdapter` added (option B): adapter is registered in composition root, all tools use it.
- [ ] `StripeObjectMapper` lives in `Infrastructure.Payments` (or scheduled for Phase 2 with explicit note).
- [ ] One commit per project (5 commits expected).
