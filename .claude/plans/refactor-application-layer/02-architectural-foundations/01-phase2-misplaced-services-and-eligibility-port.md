# Phase 2 — Move misplaced concrete services + add IEligibilityCheck port

> **Status: DONE (with deviations) — 2026-05-13.** Commits `7fc4e1cf`, `5c9d6133`, `44f6c60b`.
>
> Deviations from the original plan:
>
> - **`StripeObjectMapper` split, not full move.** Status mappers (`GetSubscriptionStatus`, `GetPaymentStatus`) take `string` and stayed in Application — they have legitimate non-Stripe-coupled consumers. Address mappers moved to `Infrastructure.Payments/Stripe/StripeAddressMapper.cs` behind a new `IStripeAddressMapper` port in Abstractions. `ProcessStripEventHandler` consumes the port. `Stripe.net` package stays in `Application.csproj` because that handler still processes raw `Stripe.Event` payloads — slice 1.8 territory.
> - **`TaxCalculationMappings` stayed put.** Plan premise (orphan + Stripe-shaped) was wrong: file is used by `InvoiceTaxApplier` and `PreviewInvoiceTaxHandler`, maps Domain↔`Abstractions.Models.Tax`↔`Shared.Models` types with no SDK coupling. Moving to Infrastructure.Tax would have created a layering violation.
> - **`IEligibilityCheck` collapsed back into `IDispatchEligibilityService`.** "Narrow port vs full surface" framing didn't survive contact with the code (single method, identical signature). Kept the established `IDispatchEligibilityService` name and moved it to `Abstractions/Dispatch/`. Single DI registration.

## Goal

Move the two genuinely-misplaced concrete services from `Application/Services/` into Infrastructure, and add the narrow `IEligibilityCheck` port that lets `Infrastructure.AI` consume dispatch eligibility without depending on `Application` directly.

## Why

Phase 2 of the master plan was deliberately **conservative**. Codex's review caught that an earlier draft was over-aggressive — most "concrete services" in `Application/Services/` are legitimate application workflows (orchestrate `ITenantUnitOfWork`, raise domain events) and **stay**.

The honest test: **does the file use a third-party SDK type, do raw I/O, or implement an Abstractions port?** If yes → Infrastructure. Otherwise → stays in Application.

By that test, only **two** files actually misplaced today.

## Files to move

| File                                                                                                           | From                                              | To                                                             | Why                               |
| -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- | -------------------------------------------------------------- | --------------------------------- |
| [StripeObjectMapper.cs](../../../../src/Core/Logistics.Application/Services/Stripe/StripeObjectMapper.cs)      | `src/Core/Logistics.Application/Services/Stripe/` | `src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/` | Imports `Stripe.*` types directly |
| [TaxCalculationMappings.cs](../../../../src/Core/Logistics.Application/Services/Tax/TaxCalculationMappings.cs) | `src/Core/Logistics.Application/Services/Tax/`    | `src/Infrastructure/Logistics.Infrastructure.Tax/`             | Maps to Stripe-shaped tax types   |

## Files that stay in Application (explicit reversal of earlier drafts)

| File                                                         | Earlier draft said         | Why it stays                                                                                                                                                            |
| ------------------------------------------------------------ | -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `DispatchEligibilityService` (244 lines)                     | Move to Domain             | Not pure entity logic — composes Load + Truck + Driver + License rules across multiple aggregates, calls `tenantUow.Repository<>`. That makes it an application policy. |
| `RuleSetSelector`                                            | Move to Domain             | If pure entity reasoning, leave it where it is. Phase 2 is not cosmetic relocation.                                                                                     |
| `MaintenanceReminderService`, `LicenseExpiryReminderService` | Move to Communications     | Application orchestration (persistence reads + domain rules + notification calls). Moving them into a capability project mixes orchestration into infrastructure.       |
| `DataExport/Deletion/Retention/ExportExpiry` services        | Move to "Compliance" infra | Same: application workflows that _call_ `IBlobStorageService` port. Stays in Application.                                                                               |
| `InvoiceTaxApplier`                                          | Move to Infrastructure.Tax | Mutates invoices and coordinates `ITaxCalculator` — application orchestration. Stays.                                                                                   |

## Plus: `IEligibilityCheck` narrow port (the design-gap fix)

`Infrastructure.AI` currently calls `IDispatchEligibilityService` from its tool layer. If the interface stays in Application (not Abstractions), `Infrastructure.AI` can't reference it cleanly.

**Resolution:** extract a thin port that Infrastructure.AI actually needs.

1. Create `src/Core/Logistics.Application.Abstractions/Dispatch/IEligibilityCheck.cs`:

   ```csharp
   namespace Logistics.Application.Abstractions.Dispatch;

   public interface IEligibilityCheck
   {
       Task<EligibilityResult> CheckAsync(
           Guid truckId, Guid loadId, Guid? driverId = null, CancellationToken ct = default);
   }
   ```

2. Move `EligibilityResult`, `EligibilityIssue`, `EligibilityIssueCode`, `EligibilitySeverity` from `IDispatchEligibilityService.cs` (Application) into `src/Core/Logistics.Application.Abstractions/Models/Dispatch/EligibilityResult.cs`.

3. Make the existing `DispatchEligibilityService` implement **both**:
   - `IDispatchEligibilityService` (used internally by Application handlers, full surface)
   - `IEligibilityCheck` (used by Infrastructure.AI, narrow surface)

4. DI registers the same concrete instance under both interfaces (or `IDispatchEligibilityService` and forward `IEligibilityCheck` to the same instance).

5. Update `Infrastructure.AI/Tools/CheckDispatchEligibilityTool.cs` to depend on `IEligibilityCheck`, not `IDispatchEligibilityService`.

Result: Application owns the policy, Infrastructure.AI talks to a narrow port, no layering violation.

## Prerequisites

- On `refactor/application-abstractions` at or after commit `00ddc2a1`.
- Slice 1.8 not required. Slice 1.9-remainder for Infrastructure.AI may overlap — coordinate if running in parallel.

## Step-by-step

### Move 1: `StripeObjectMapper.cs`

1. `git mv src/Core/Logistics.Application/Services/Stripe/StripeObjectMapper.cs src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeObjectMapper.cs`
2. Update namespace: `Logistics.Application.Services` → `Logistics.Infrastructure.Payments.Stripe`
3. `grep -rln "StripeObjectMapper\|ToStripeAddressOptions\|ToAddressEntity\|GetSubscriptionStatus\|GetPaymentStatus" src/` — find consumers
4. Update consumers' `using` statements
5. If any consumers are now in projects outside `Infrastructure.Payments`, that's a layering smell — they shouldn't be using a Stripe mapper. Audit each.
6. Build → commit

### Move 2: `TaxCalculationMappings.cs`

Same pattern.

1. `git mv src/Core/Logistics.Application/Services/Tax/TaxCalculationMappings.cs src/Infrastructure/Logistics.Infrastructure.Tax/TaxCalculationMappings.cs`
2. Update namespace
3. Update consumers
4. Build → commit

### Add: `IEligibilityCheck` port

1. Create `Abstractions/Dispatch/IEligibilityCheck.cs` with the narrow interface.
2. Extract `EligibilityResult` + supporting types from `IDispatchEligibilityService.cs` into `Abstractions/Models/Dispatch/EligibilityResult.cs`. Update namespace.
3. Update `IDispatchEligibilityService.cs` (in Application) to reference the moved DTOs via `using Logistics.Application.Abstractions.Models.Dispatch;`.
4. Update `DispatchEligibilityService.cs` to implement `IEligibilityCheck` in addition to `IDispatchEligibilityService`. Single-method addition; `IEligibilityCheck.CheckAsync` can be `public Task<EligibilityResult> CheckAsync(...)` and the existing method body satisfies both interfaces (same signature).
5. Register: in `Application/Registrar.cs`, `services.AddScoped<IDispatchEligibilityService, DispatchEligibilityService>();` plus `services.AddScoped<IEligibilityCheck>(sp => sp.GetRequiredService<IDispatchEligibilityService>());`.
6. Update `Infrastructure.AI/Tools/CheckDispatchEligibilityTool.cs` to depend on `IEligibilityCheck`.
7. Build → commit.

## Critical files

- `src/Core/Logistics.Application/Services/Stripe/StripeObjectMapper.cs` → move
- `src/Core/Logistics.Application/Services/Tax/TaxCalculationMappings.cs` → move
- `src/Core/Logistics.Application/Services/AiDispatch/IDispatchEligibilityService.cs` — split out DTOs
- `src/Core/Logistics.Application/Services/AiDispatch/DispatchEligibilityService.cs` — add second interface implementation
- New: `src/Core/Logistics.Application.Abstractions/Dispatch/IEligibilityCheck.cs`
- New: `src/Core/Logistics.Application.Abstractions/Models/Dispatch/EligibilityResult.cs`
- `src/Core/Logistics.Application/Registrar.cs` — DI registration
- `src/Infrastructure/Logistics.Infrastructure.AI/Tools/CheckDispatchEligibilityTool.cs` — switch port

## Verification

- `grep -rn "using Stripe" src/Core/Logistics.Application/` → 0
- `grep -rn "using Stripe" src/Core/Logistics.Application.Abstractions/` → still expects 0 (already true after Slice 1.7; Slice 1.8 will keep it at 0)
- `dotnet build Logistics.slnx` → 0 errors
- Tests pass

## Risks

| Risk                                                                                                         | Mitigation                                                                                                                                                                                      |
| ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Moving `StripeObjectMapper` orphans extensions still imported via `Logistics.Application.Services` namespace | Consumers updated to import `Logistics.Infrastructure.Payments.Stripe`. If anything outside `Infrastructure.Payments` still uses these mappers, that's a layering bug — flag, don't paper over. |
| `IEligibilityCheck` and `IDispatchEligibilityService` have the same method signature — confusing             | The narrow port is a strict subset of the full service. Document at the interface level why both exist.                                                                                         |

## Acceptance

- [ ] `StripeObjectMapper.cs` lives in `Infrastructure.Payments/Stripe/`.
- [ ] `TaxCalculationMappings.cs` lives in `Infrastructure.Tax/`.
- [ ] `IEligibilityCheck` exists in `Abstractions/Dispatch/`.
- [ ] `EligibilityResult` and supporting types live in `Abstractions/Models/Dispatch/`.
- [ ] `Infrastructure.AI/Tools/CheckDispatchEligibilityTool.cs` consumes `IEligibilityCheck` only.
- [ ] DI registers `DispatchEligibilityService` as both interfaces.
- [ ] Build green, tests pass.
- [ ] 3 commits (one per move/add).
