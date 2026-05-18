# Abstractions → Domain decoupling

> **Status (2026-05-13): DEFERRED.** Two reasons:
>
> 1. **Heavy overlap with Slice 1.8 on the Stripe ports.** The 8 Stripe interfaces take Domain entities (`Tenant`, `Employee`, `Payment`, `SubscriptionPlan`, `Subscription`) AND return Stripe SDK types. If 1.8 doesn't run, decoupling Domain piecemeal on the Stripe ports is wasted work — the Stripe ports stay coupled either way.
> 2. **User decision on 1.8.** Slice 1.8 was deferred for risk/benefit reasons. Running this slice without 1.8 means doing the entity→snapshot half of the work, leaving the SDK-type half in place — a half-decoupled posture that doesn't pay off.
>
> Non-Stripe ports that reference Domain types could be decoupled independently (Tenancy `ICurrentTenantAccessor.GetCurrentTenant() → Tenant`, etc.), but the value is limited until the full Abstractions→Domain reference can actually be dropped from `Abstractions.csproj`. Re-open this slice when (a) Slice 1.8 also runs, or (b) the team decides to do the non-Stripe portion only and documents Stripe ports as an explicit Domain-coupling exception.
>
> Consequences carried forward:
>
> - `Abstractions.csproj` still has `<ProjectReference Include="..\Logistics.Domain\Logistics.Domain.csproj" />` and `<ProjectReference Include="..\Logistics.Domain.Primitives\Logistics.Domain.Primitives.csproj" />`.
> - Domain entity types appear in port surfaces across `Abstractions/{Tenancy,Payments,Geocoding,Eld,Routing,Documents,LoadBoard,Tax,AiDispatch,Notifications,Realtime}/`.
> - Infrastructure projects that implement one narrow port (e.g., `IBlobStorageService`) continue to see the entire Domain assembly transitively. The Phase 5 arch test for `Abstractions_does_not_reference_Domain` cannot pass yet.
>
> Body below is the original plan and remains usable.

## Goal

Remove `<ProjectReference Include="..\Logistics.Domain\Logistics.Domain.csproj" />` from `Logistics.Application.Abstractions.csproj`. Replace every domain-entity type that appears in a port signature with a DTO/snapshot in `Abstractions/Models/`.

## Why

After Slice 1.2, `Abstractions.csproj` references `Logistics.Domain` because `ICurrentTenantAccessor.GetCurrentTenant()` returns a `Tenant` entity, and several other ports pass entities through.

This is the **pragmatic** choice (matches Ardalis / eShop precedent) but the **purist** Clean Architecture posture is "Abstractions = ports + DTOs only." The purist version costs a few snapshot types and mappings but buys real value:

- **Blast radius.** Infrastructure projects that implement one narrow port (e.g., `IBlobStorageService`) currently see the entire Domain assembly transitively. They could accidentally reach into `Trip`, `Load`, `Driver`, etc.
- **Port surface.** `GetCurrentTenant() → Tenant` exposes every method on the `Tenant` entity (e.g., `UpdateSubscription`, `Activate`) when callers usually just need `{ Id, Name, IsActive, SubscriptionPlanId }`.
- **Lazy-loading footgun.** Holding a Domain entity in Infrastructure tempts `.Subscription`-style lazy-load chains through whatever DbContext is in scope, sometimes the wrong one.
- **Microsoft.Extensions.\*.Abstractions style.** This is the canonical .NET pattern (`IConfiguration`, `ILogger<>`, `IServiceProvider` etc. have no domain coupling).

## Prerequisites

- On `refactor/application-abstractions` at or after commit `00ddc2a1` (Slice 1.9-partial).
- Best done **after** Slice 1.8 (Stripe DTOs land), so the Stripe ports already follow the snapshot pattern and there's a clear precedent. But this slice can also precede 1.8 if you prefer — they don't conflict.

## Audit checklist (do this first)

Identify every entity type that crosses an Abstractions port surface:

```bash
# Domain entity types referenced in Abstractions
grep -rh "Logistics\\.Domain\\.Entities\\." src/Core/Logistics.Application.Abstractions --include='*.cs' | sort -u

# Domain value-object types referenced
grep -rh "Logistics\\.Domain\\.Primitives\\.ValueObjects\\." src/Core/Logistics.Application.Abstractions --include='*.cs' | sort -u

# Any other Logistics.Domain.* references
grep -rn "Logistics\\.Domain" src/Core/Logistics.Application.Abstractions --include='*.cs'
```

Expected hits (from current state — verify):

| Surface                                                                                                                              | Entity type                                  | Likely fix                                                       |
| ------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------- | ---------------------------------------------------------------- |
| `ICurrentTenantAccessor.GetCurrentTenant()`                                                                                          | `Tenant`                                     | New `TenantSnapshot` record in `Abstractions/Models/Tenancy/`    |
| `IStripeConnectService.CreateConnectedAccountAsync(Tenant tenant)`                                                                   | `Tenant` param                               | `TenantSnapshot` or a smaller `ConnectAccountCreateRequest`      |
| `IStripeConnectService.CreateEmployeeConnectedAccountAsync(Employee employee, Address fallbackAddress)`                              | `Employee`, `Address`                        | `EmployeePayoutOnboardingRequest` + `AddressDto`                 |
| `IStripeCustomerService.*(Tenant tenant)`                                                                                            | `Tenant`                                     | Same `TenantSnapshot`                                            |
| `IStripePaymentService.CreatePaymentIntentAsync(Payment payment, Tenant tenant)`                                                     | `Payment`, `Tenant`                          | `PaymentIntentCreateRequest` DTO that flattens what Stripe needs |
| `IStripePlanService.CreatePlanAsync(SubscriptionPlan plan)`                                                                          | `SubscriptionPlan`                           | `SubscriptionPlanDescriptor`                                     |
| `IStripeSubscriptionService.*(SubscriptionPlan, Tenant, ...)`                                                                        | `SubscriptionPlan`, `Tenant`, `Subscription` | Multiple DTOs                                                    |
| Boundary DTOs in Abstractions/Models that import `Logistics.Domain.Primitives.ValueObjects.Address` (e.g., `CheckoutSessionRequest`) | `Address` value object                       | `AddressDto`                                                     |
| Boundary DTOs that import `Money` value object                                                                                       | `Money`                                      | `MoneyDto` (or `decimal Amount` + `string Currency`)             |

## Suggested DTO sketches

```csharp
// Abstractions/Models/Tenancy/TenantSnapshot.cs
public sealed record TenantSnapshot(
    Guid Id,
    string Name,
    string CompanyName,
    string? Email,
    string? CountryCode,
    Guid? SubscriptionPlanId,
    bool IsActive,
    string? StripeCustomerId,
    string? StripeConnectAccountId);

// Abstractions/Models/Tenancy/AddressDto.cs
public sealed record AddressDto(
    string? Line1, string? Line2, string? City, string? State, string? ZipCode, string? Country);

// Abstractions/Models/Tenancy/MoneyDto.cs (only if Stripe DTOs need it; otherwise inline)
public sealed record MoneyDto(decimal Amount, string Currency);

// Abstractions/Models/Payroll/EmployeeSnapshot.cs (if employee crosses ports)
public sealed record EmployeeSnapshot(Guid Id, string FullName, string? Email, AddressDto? Address);

// Abstractions/Models/Payments/SubscriptionPlanSnapshot.cs
public sealed record SubscriptionPlanSnapshot(
    Guid Id, string Name, MoneyDto BasePrice, MoneyDto PerTruckPrice, MoneyDto? AiOveragePrice, string? StripeProductId);
```

These are **starting points**. The fields must be derived from a real audit of what callers consume on the entity. Don't speculate.

## Step-by-step

1. **Audit pass.** Use the greps above. Produce a notes file (e.g., `/tmp/abstractions-domain-audit.md`) listing every entity-referencing port surface and what fields callers read.
2. **Create DTOs** in `Abstractions/Models/{Area}/`. Add them in one prep PR if you want a clean separation, or as part of each port's PR.
3. **For each port that takes/returns an entity:**
   - Update the signature to use the DTO
   - At the **caller side** (in Application handlers), introduce a mapping: `tenant.ToSnapshot()` (using Mapperly per project convention — `src/Core/Logistics.Mappings/`)
   - Update the implementation in Infrastructure to consume the DTO directly
   - Commit
4. **Remove the `Logistics.Domain` project reference** from `Abstractions.csproj`. The build will tell you about any straggler `using Logistics.Domain.*;` lines in Abstractions — they should all be gone after step 3.
5. **Re-check Domain.Primitives reference.** Decide: do you also drop `Logistics.Domain.Primitives`? If `Address`, `Money`, etc. are now `AddressDto`/`MoneyDto`, you can. But primitive value objects often legitimately appear in DTOs — judgment call.
6. **Run all tests.**

## Mapping pattern

Use Mapperly per project convention (see `.claude/rules/backend/mapperly.md`). Add an extension `Tenant.ToSnapshot()` in `Logistics.Mappings`:

```csharp
[Mapper]
public static partial class TenantSnapshotMapper
{
    [MapperIgnoreSource(nameof(Tenant.Subscription))]
    [MapperIgnoreSource(nameof(Tenant.Employees))]
    [MapperIgnoreSource(nameof(Tenant.Loads))]
    public static partial TenantSnapshot ToSnapshot(this Tenant tenant);
}
```

## Critical files

- `src/Core/Logistics.Application.Abstractions/Logistics.Application.Abstractions.csproj` — final ref removal
- Every `I*.cs` in `Abstractions/{Tenancy,Payments,...}/` that has an entity type in its signature
- New DTOs in `Abstractions/Models/{Tenancy,Payments,Payroll,...}/`
- New mappers in `src/Core/Logistics.Mappings/`
- Every handler that calls a now-DTO-fied port — needs `.ToSnapshot()` mapping at the call site

## Verification

- `grep -rn "Logistics\\.Domain\\." src/Core/Logistics.Application.Abstractions --include='*.cs'` returns **zero**.
- `grep -n "Logistics\\.Domain" src/Core/Logistics.Application.Abstractions/Logistics.Application.Abstractions.csproj` returns **zero**.
- `dotnet build Logistics.slnx` — 0 errors.
- All tests pass.

## Risks

| Risk                                                                                         | Mitigation                                                                                                              |
| -------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Forgot a field in a snapshot → silent feature regression                                     | Compare snapshot fields against every grep hit of the entity field name in consumer code. Field-for-field audit.        |
| Mapper allocation per request — perf hit                                                     | Mapperly generates source-gen mappers (no reflection). Verify the generated code in `obj/Debug/.../*.g.cs` looks clean. |
| Coordination with Slice 1.8 (Stripe DTOs) — overlap on `Tenant`/`SubscriptionPlan` snapshots | Pick an order. If 1.8 runs first, this slice reuses those DTOs. If this runs first, 1.8 reuses these.                   |

## Acceptance

- [ ] `Abstractions.csproj` has no `Logistics.Domain*` project reference.
- [ ] No `using Logistics.Domain.*;` lines in `Application.Abstractions/`.
- [ ] Every former entity-in-port-signature now uses a DTO/snapshot from `Abstractions/Models/`.
- [ ] Mappers in `Logistics.Mappings` cover every snapshot.
- [ ] Application handlers map entity → snapshot at the call site, not inside ports.
- [ ] Build green, tests pass.
