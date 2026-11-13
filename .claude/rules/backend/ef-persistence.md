---
paths:
  - "src/Core/Logistics.Domain/**/*.cs"
  - "src/Core/Logistics.Application/**/*.cs"
  - "src/Infrastructure/**/*.cs"
---

# EF Persistence

## Add new entities explicitly

Ids are pre-generated, so a new entity reached only through a tracked parent's collection is saved as an UPDATE of 0 rows and throws a misleading `DbUpdateConcurrencyException`. Call `AddAsync`:

```csharp
var message = conversation.AddTextMessage(sequence, role, text);
await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
await tenantUow.SaveChangesAsync(ct);
```

Exempt only when the parent itself is new and added as a graph (`Load.Create`). `NavigationDiscoveryGuard` turns the mistake into a named error; fix the call site, don't remove the guard.

Tenant entities have no concurrency tokens, so a concurrency exception on tenant data means a missing row. Only master `User` / `AppRole` can genuinely conflict.

## Cascade deletes are deliberate

Deleting a customer, load, employee, truck or conversation removes everything under it. Don't flag cascades as data loss or propose `Restrict`, soft delete or delete guards. A new optional reference gets a real FK (`SetNull`/`Cascade`), not a loose `Guid`.

## Lazy loading

On. Never `.Include()`. Reading a navigation inside a mapper or loop is an N+1; batch it (see `mapperly.md`).

## Units and types

- Every `decimal` defaults to `numeric(18,2)` and `Money.Currency` to length 3. Declare `HasPrecision` only for other scales: per-km rates `(18,4)`, fuel volume `(18,3)`, AI cost `(18,6)`.
- Internal amounts are plain `decimal` in `Tenant.Currency`. `Money` is only for amounts that arrive in another currency (fuel cards, load board, negotiation, plans). Never sum `Money` without checking currency.
- Physical quantities use fixed units with the unit in the name: meters (`OdometerMeters` is `long`), kilograms (`WeightKg`), per-km rates (`RatePerKm`). Convert for display with `DistanceUnitExtensions`; imperial input goes through `ImperialUnits` at the integration edge. Only IFTA output stays in miles.
- Instants are `XxxAt` (`DateTime`, UTC, `timestamptz`). Calendar days are `DateOnly`, sent as `yyyy-MM-dd` via `DateUtils.toDateOnly`. Durations are integer `XxxMinutes`.
- A string holding JSON maps to `jsonb`, unless it is a payload replayed verbatim.
- **Some unique indexes live only in migrations.** IFTA tax rate `(Year, Quarter, Jurisdiction)` and mileage `(Truck, Date, Jurisdiction)` are raw-SQL `NULLS NOT DISTINCT` indexes, because complex-type members can't go in an EF index. A model rebuild won't recreate them, and `TruckLocationRecorder`'s `ON CONFLICT` upsert needs the mileage one.
