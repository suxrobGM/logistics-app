---
paths:
  - "src/Core/Logistics.Domain/**/*.cs"
  - "src/Core/Logistics.Application/**/*.cs"
  - "src/Infrastructure/**/*.cs"
---

# EF Persistence Traps

## New entities MUST be registered with `repository.AddAsync`

Entity ids are pre-generated (`Guid.NewGuid()` in the `Entity` base), so EF treats a new entity
that reaches the tracker only via a tracked parent's navigation collection as an existing row and
saves an UPDATE affecting 0 rows - a misleading `DbUpdateConcurrencyException`.

```csharp
var sequence = await tenantUow.ReserveSequencesAsync(conversation, 1, ct);
var message = conversation.AddTextMessage(sequence, role, text);
await tenantUow.Repository<AgentMessage>().AddAsync(message, ct); // without this, save throws
await tenantUow.SaveChangesAsync(ct);
```

Exempt only when the parent itself is new and added as a graph (`Load.Create` / `Trip.Create` -
the cascade marks everything Added). `NavigationDiscoveryGuard` (attached to both DbContexts)
turns the mistake into an immediate, named error - fix the call site, don't remove the guard.

## Concurrency-token asymmetry

Tenant entities have no concurrency tokens - a `DbUpdateConcurrencyException` on tenant data means
a missing row (usually this trap), not a write conflict. Only master `User` / `AppRole` carry a
`ConcurrencyStamp` and can conflict for real.

## Decimal and currency defaults

Both DbContexts set every `decimal` to `numeric(18,2)` and every `Money.Currency` to length 3 in
`ConfigureConventions`, so don't repeat `HasPrecision(18, 2)`. A column that needs another scale
says so with `HasPrecision(p, s)`: per-km rates `(18,4)`, fuel volume `(18,3)`, AI cost `(18,6)`.
Never leave a column as unbounded `numeric`.

Tenant-internal amounts are plain `decimal` in `Tenant.Currency`. `Money` is only for money that
arrives in another currency (fuel card transactions, load board listings and negotiation offers,
subscription plans); never add a `Money` property to an internal amount, and never sum `Money`
across rows without checking the currency.

Physical quantities are stored in fixed units, never the tenant's: distance and length in meters,
weight in kilograms, odometers in meters (`long`), per-distance rates per km at `HasPrecision(18, 4)`.
Name the unit in the property (`WeightKg`, `RatePerKm`, `OdometerMeters`). Convert for display with
`DistanceUnitExtensions` (`FromMeters`, `RateFromPerKm`) or on the client; external miles, pounds
and feet go through `ImperialUnits` at the integration edge. IFTA output alone stays in miles.

## Time and JSON columns

Name an instant `XxxAt` (`DateTime`, UTC, `timestamptz`): `RecordedAt`, `OccurredAt`, `StartedAt`. A
calendar day with no time is `DateOnly` (`date`), e.g. `ExpenseDate`, `DueDate`; the client sends it as
`yyyy-MM-dd` through `DateUtils.toDateOnly`. Durations are integer minutes named `XxxMinutes`. A string
property holding JSON maps to `jsonb` with `HasColumnType("jsonb")`, except a payload replayed verbatim.
