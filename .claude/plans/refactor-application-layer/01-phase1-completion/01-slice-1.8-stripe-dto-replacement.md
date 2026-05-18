# Slice 1.8 — Replace Stripe SDK types in port signatures with DTOs

> **Status (2026-05-13): DEFERRED.** User chose not to pursue this slice. The rationale: per-port DTO redesign is a real coding session of API design decisions (not a mechanical relocation), the risk of subtly missing a webhook field is high, and the value of dropping `Stripe.net` from `Abstractions.csproj` did not justify that risk for the team's current priorities.
>
> Consequences carried forward:
>
> - `Abstractions.csproj` still has `<PackageReference Include="Stripe.net" Version="50.2.0" />`.
> - The 8 port interfaces under `src/Core/Logistics.Application.Abstractions/Payments/Stripe/` still expose Stripe SDK types (`Account`, `Session`, `PaymentIntent`, etc.) in their signatures.
> - The Phase 5 arch test that would forbid third-party SDK types in `Abstractions/Payments/Stripe/*.cs` must be written with an allow-list — or this slice runs before that arch test lands.
> - This slice overlaps heavily with [03-abstractions-domain-decoupling.md](03-abstractions-domain-decoupling.md) because the Stripe ports also accept Domain entities. Whichever runs first defines the DTOs the other reuses.
>
> Re-open this slice when the team revisits Stripe model decoupling. The body below is the original plan and remains usable.

## Goal

Remove `Stripe.*` SDK types from the public surface of [Logistics.Application.Abstractions/Payments/Stripe/](../../../../src/Core/Logistics.Application.Abstractions/Payments/Stripe/) port interfaces. Replace each SDK return type with a hand-designed DTO in `Abstractions/Models/Payments/`. After this slice, drop `<PackageReference Include="Stripe.net" />` from `Abstractions.csproj`.

## Why

Slice 1.7 relocated the 8 Stripe ports but kept their SDK return types as a deliberate intermediate step. Right now `Abstractions.csproj` depends on `Stripe.net 50.2.0`, which means every Infrastructure project that consumes any Stripe port transitively links the Stripe SDK — same coupling we were trying to avoid. The arch test in Phase 5 will fail until this slice lands.

## Why this is the high-risk slice

Unlike Slices 1.1–1.7 (mechanical relocations), this is **API redesign per port**. Each DTO is a design decision: which SDK fields to expose, how to model nullable/optional, what to rename. Get a field wrong and Stripe webhooks, Connect onboarding, Customer Portal, or subscription renewals can silently misbehave. Treat as a real coding session, not a script run.

## Prerequisites

- On `refactor/application-abstractions` at or after commit `9d0cbe7a` (Slice 1.7).
- `Abstractions.csproj` currently has `Stripe.net` package reference.
- Slice 1.9-remainder is NOT a prerequisite — they can be done in either order.

## Scope

### In scope

8 port interfaces in [src/Core/Logistics.Application.Abstractions/Payments/Stripe/](../../../../src/Core/Logistics.Application.Abstractions/Payments/Stripe/) — replace these SDK return/parameter types:

| Interface                    | SDK types to replace                                                    |
| ---------------------------- | ----------------------------------------------------------------------- |
| `IStripeConnectService`      | `Account`, `AccountLink`, `Session` (Checkout), `Transfer`, `LoginLink` |
| `IStripeCustomerService`     | `Stripe.Customer`                                                       |
| `IStripePaymentService`      | `SetupIntent`, `PaymentIntent`                                          |
| `IStripePlanService`         | `StripePlanResult` record contains `Product`, `Price`                   |
| `IStripePortalService`       | none — returns `string`                                                 |
| `IStripeService`             | none — string property                                                  |
| `IStripeSubscriptionService` | `Stripe.Subscription`, `SubscriptionItem`                               |
| `IStripeUsageService`        | none                                                                    |

### Out of scope (defer)

- Renaming the interfaces themselves (`IStripeXService` stays — per Phase 1 directional decision, hide SDK types only)
- Moving `StripeObjectMapper` out of Application (that's Phase 2)
- Anything in `Infrastructure.Payments` beyond what's required to make the new DTOs compile

## Method

For each SDK type:

1. **Audit consumers.** `grep -rn "<SdkTypeName>" src/` — list everywhere the SDK type appears in handler/consumer code. The DTO must expose **only** the fields callers actually read.
2. **Design the DTO** as a `record` in `src/Core/Logistics.Application.Abstractions/Models/Payments/`. Name suggestion: `{SdkTypeName}Descriptor` or domain-flavored name (e.g., `PaymentIntentDescriptor`, `ConnectAccountDescriptor`, `CheckoutSessionDescriptor`).
3. **Update the port signature** to return the DTO.
4. **Add a mapper** in `Infrastructure.Payments` (e.g., `StripeMappers.cs`) that converts SDK → DTO. Keep the mapper internal to Infrastructure.Payments.
5. **Update the service implementation** to call the mapper before returning.
6. **Update consumer code** to use DTO field names. If a caller reads a field the DTO doesn't expose, add it (don't just expose the SDK type back).

## Suggested DTO sketches (starting point — refine per audit)

```csharp
// Abstractions/Models/Payments/ConnectAccountDescriptor.cs
public sealed record ConnectAccountDescriptor(
    string Id,
    string Country,
    string? Email,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool DetailsSubmitted,
    IReadOnlyList<string> RequirementsCurrentlyDue,
    IReadOnlyList<string> RequirementsPastDue,
    string DisabledReason);

// Abstractions/Models/Payments/AccountLinkDescriptor.cs
public sealed record AccountLinkDescriptor(string Url, DateTimeOffset ExpiresAt);

// Abstractions/Models/Payments/CheckoutSessionDescriptor.cs
public sealed record CheckoutSessionDescriptor(
    string Id, string Url, string? PaymentIntentId, IReadOnlyDictionary<string, string> Metadata);

// Abstractions/Models/Payments/TransferDescriptor.cs
public sealed record TransferDescriptor(string Id, long AmountInSmallestUnit, string Currency, string DestinationAccountId);

// Abstractions/Models/Payments/PaymentIntentDescriptor.cs
public sealed record PaymentIntentDescriptor(
    string Id, string Status, long AmountInSmallestUnit, string Currency, string? ClientSecret, IReadOnlyDictionary<string, string> Metadata);

// Abstractions/Models/Payments/SetupIntentDescriptor.cs
public sealed record SetupIntentDescriptor(string Id, string Status, string? ClientSecret);

// Abstractions/Models/Payments/StripeCustomerDescriptor.cs
public sealed record StripeCustomerDescriptor(string Id, string? Email, string? Name, IReadOnlyDictionary<string, string> Metadata);

// Abstractions/Models/Payments/SubscriptionDescriptor.cs
public sealed record SubscriptionDescriptor(
    string Id, string Status, DateTimeOffset? CurrentPeriodEnd, DateTimeOffset? CancelAt, bool CancelAtPeriodEnd,
    IReadOnlyList<SubscriptionItemDescriptor> Items);

public sealed record SubscriptionItemDescriptor(string Id, string PriceId, long? Quantity);

// Abstractions/Models/Payments/StripePlanDescriptor.cs (replaces StripePlanResult)
public sealed record StripePlanDescriptor(
    string ProductId, PriceDescriptor BasePrice, PriceDescriptor PerTruckPrice, PriceDescriptor? AiOveragePrice);

public sealed record PriceDescriptor(string Id, long UnitAmountInSmallestUnit, string Currency, string? RecurringInterval);

// Abstractions/Models/Payments/LoginLinkDescriptor.cs
public sealed record LoginLinkDescriptor(string Url);
```

Audit each consumer site **first** — add only fields actually consumed. Don't speculatively expose.

## Step-by-step

1. **Audit all consumers** of every Stripe SDK type listed above. Produce a notes file (`/tmp/stripe-dto-audit.md` is fine) mapping each SDK field used → which DTO field will replace it.
2. **Create the DTOs** in `Abstractions/Models/Payments/`.
3. **For each port interface, one PR:**
   - Update signature to use the new DTO
   - Add/update mapper in `Infrastructure.Payments`
   - Update service implementation to call mapper before returning
   - Update every consumer in handlers
   - Build + run any related tests
   - Commit
4. **After all 8 are done:**
   - Remove `<PackageReference Include="Stripe.net" Version="50.2.0" />` from `Abstractions.csproj`
   - Build to confirm nothing in Abstractions still references Stripe.\*
   - Commit
5. **Run the Stripe smoke tests** if they exist (`test/Logistics.Infrastructure.Payments.Tests/`). Manually test webhook + subscription create + checkout if a sandbox is available.

## Critical files

- All `src/Core/Logistics.Application.Abstractions/Payments/Stripe/I*.cs` (8 files)
- `src/Core/Logistics.Application.Abstractions/Logistics.Application.Abstractions.csproj` — drop Stripe.net package ref at the end
- `src/Core/Logistics.Application.Abstractions/Models/Payments/` — new DTOs
- `src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/Stripe*.cs` — implementations + new mapper
- All handler files under `src/Core/Logistics.Application/Commands/{Payment,Subscription,StripeConnect,Webhooks,Tenant}/` — consumer updates
- `test/Logistics.Infrastructure.Payments.Tests/` — update fixtures/assertions

## Verification

- `dotnet build Logistics.slnx` — 0 errors after each per-port PR.
- `grep -rn "using Stripe;" src/Core/Logistics.Application.Abstractions/` returns **zero** matches (after the final cleanup PR).
- `grep -rn "Stripe\\." src/Core/Logistics.Application/Commands src/Core/Logistics.Application/Queries` returns **zero** matches — handlers must speak only in DTOs now.
- `test/Logistics.Infrastructure.Payments.Tests` pass.
- Smoke test (manual or scripted): create a checkout session, hit the local webhook endpoint with a Stripe CLI forward, verify it processes.

## Risks

| Risk                                                                                                               | Mitigation                                                                                                                              |
| ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- |
| Webhook payload field missed in DTO → silent processing failure                                                    | The `ProcessStripEventHandler` is the highest-value consumer to audit. Cross-reference every webhook event field read with a DTO field. |
| Connect onboarding flow breaks (multi-step UI)                                                                     | Manual test the onboarding link → return → status update loop end-to-end.                                                               |
| Subscription renewal logic uses a Stripe field we forgot to expose                                                 | Step through `RenewSubscriptionAsync`, `ChangeSubscriptionPlanAsync` carefully. Compare with `StripeWebhookHandler`'s expected fields.  |
| `Metadata` is `IReadOnlyDictionary` in DTO but `Dictionary<string,string>` on SDK type — null vs empty differences | Always map to empty dict in mapper, never null.                                                                                         |

## Acceptance

- [ ] All 8 port interfaces speak only in DTOs (no `Stripe.*` types in signatures or fields).
- [ ] `Abstractions.csproj` has no `Stripe.net` package reference.
- [ ] `grep` confirms no `using Stripe;` or `Stripe.X` references in `Application.Abstractions` or `Application` (except `StripeObjectMapper`, which Phase 2 moves out).
- [ ] All `Infrastructure.Payments.Tests` pass.
- [ ] Manual smoke: a Stripe Checkout session can be created and webhook-completed end-to-end.
- [ ] Each per-port commit has its own descriptive message; one final commit removes the `Stripe.net` package ref.
