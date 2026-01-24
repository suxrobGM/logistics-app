---
name: backend-patterns
description: Reference guide for DDD, CQRS, and domain event patterns with code examples
---

# Backend Architecture Reference

## Layer Structure

```text
Presentation (HTTP)     → Logistics.API, Logistics.IdentityServer, Logistics.DbMigrator
Application (Business)  → Commands/Queries (MediatR), Services, SignalR Hubs
Domain (Entities)       → Entities, Domain Events, Specifications, Value Objects
Infrastructure (Data)   → EF Core DbContext, Repositories, Unit of Work, External Integrations
Shared (Models)         → DTOs shared between backend and frontend
```

## Key Directories

| Layer | Location |
|-------|----------|
| Commands | `src/Core/Logistics.Application/Commands/` |
| Queries | `src/Core/Logistics.Application/Queries/` |
| Domain Events | `src/Core/Logistics.Domain/Events/` |
| Enums | `src/Core/Logistics.Domain.Primitives/Enums/` |
| Event Handlers | `src/Core/Logistics.Application/Events/` |
| Entities | `src/Core/Logistics.Domain/Entities/` |
| Specifications | `src/Core/Logistics.Domain/Specifications/` |
| Mappers | `src/Core/Logistics.Mappings/` |
| DTOs | `src/Core/Logistics.Shared.Models/` |

## Domain Events - Full Example

Domain events decouple notification logic from command handlers.

### 1. Define the Event

```csharp
// Location: src/Core/Logistics.Domain/Events/LoadAssignedToTruckEvent.cs
public record LoadAssignedToTruckEvent(
    Guid LoadId,
    long LoadNumber,
    Guid TruckId,
    string TruckNumber,
    string? DriverDeviceToken,
    string DriverDisplayName) : IDomainEvent;
```

### 2. Raise Event in Entity

```csharp
// Location: src/Core/Logistics.Domain/Entities/Load.cs
public void AssignToTruck(Truck truck)
{
    AssignedTruckId = truck.Id;
    AssignedTruck = truck;

    DomainEvents.Add(new LoadAssignedToTruckEvent(
        Id, Number, truck.Id, truck.Number,
        truck.MainDriver?.DeviceToken,
        truck.MainDriver?.GetFullName() ?? truck.Number));
}
```

### 3. Handle Event

```csharp
// Location: src/Core/Logistics.Application/Events/LoadAssignedToTruckNotificationHandler.cs
internal sealed class LoadAssignedToTruckNotificationHandler(
    IPushNotificationService pushService,
    INotificationService notificationService)
    : IDomainEventHandler<LoadAssignedToTruckEvent>
{
    public async Task Handle(LoadAssignedToTruckEvent @event, CancellationToken ct)
    {
        // Push notification to driver's device
        if (!string.IsNullOrEmpty(@event.DriverDeviceToken))
        {
            await pushService.SendNotificationAsync(
                @event.DriverDeviceToken,
                "New Load Assigned",
                $"Load #{@event.LoadNumber} has been assigned to you",
                ct);
        }

        // In-app notification for TMS users
        await notificationService.SendNotificationAsync(
            $"Load #{@event.LoadNumber} assigned to {truck.Number}",
            NotificationType.LoadAssigned,
            ct);
    }
}
```

### 4. Event Dispatch

Events are automatically dispatched by `DispatchDomainEventsInterceptor` during `SaveChanges()`.

## Existing Domain Events

| Event | Trigger | Purpose |
|-------|---------|---------|
| `LoadAssignedToTruckEvent` | `Load.AssignToTruck()` | Push to driver, in-app notification |
| `TripAssignedToTruckEvent` | `Trip.AssignToTruck()` | Push to new/old driver, in-app notification |

## Multi-Tenant Context

- **Master DB**: `MasterDbContext` - Tenants, Subscriptions, SuperAdmin users
- **Tenant DB**: `TenantDbContext` - All tenant-specific data
- Tenant context is resolved via `ITenantResolver` from JWT claims
