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
var message = conversation.AddTextMessage(role, text);
await tenantUow.Repository<AgentMessage>().AddAsync(message, ct); // without this, save throws
await tenantUow.SaveChangesAsync(ct);
```

Exempt only when the parent itself is new and added as a graph (`LoadFactory` / `TripFactory` -
the cascade marks everything Added). `NavigationDiscoveryGuard` (attached to both DbContexts)
turns the mistake into an immediate, named error - fix the call site, don't remove the guard.

## Concurrency-token asymmetry

Tenant entities have no concurrency tokens - a `DbUpdateConcurrencyException` on tenant data means
a missing row (usually this trap), not a write conflict. Only master `User` / `AppRole` carry a
`ConcurrencyStamp` and can conflict for real.
