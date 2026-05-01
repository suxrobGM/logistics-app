# Domain Model

The domain layer (`src/Core/Logistics.Domain/`) holds entities, aggregates, value objects, domain events, and specifications. Entities are split into two databases by marker interface:

- `IMasterEntity` - lives in the **master DB** (platform state shared across tenants).
- `ITenantEntity` - lives in **tenant DBs** (per-company operational data).

A few entities (notably `User` and `Invoice`) implement both because they need to be visible from either side.

## Base abstractions

```mermaid
classDiagram
    class IEntity~TKey~ {
        <<interface>>
        +TKey Id
    }
    class IAuditableEntity {
        <<interface>>
        +DateTime CreatedAt
        +string? CreatedBy
        +DateTime? UpdatedAt
        +string? UpdatedBy
    }
    class IMasterEntity {
        <<marker>>
    }
    class ITenantEntity {
        <<marker>>
    }
    class IDomainEvent {
        <<interface>>
    }

    class Entity {
        +Guid Id
        +List~IDomainEvent~ DomainEvents
    }
    class AuditableEntity {
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +SetCreated(userId)
        +SetUpdated(userId)
    }

    Entity ..|> IEntity
    AuditableEntity --|> Entity
    AuditableEntity ..|> IAuditableEntity
```

`Entity` provides the GUID primary key and the `DomainEvents` list. `AuditableEntity` adds the four audit fields, populated by an EF Core SaveChanges interceptor in `Logistics.Infrastructure.Persistence`. The marker interfaces (`IMasterEntity`, `ITenantEntity`) drive which `DbContext` (`MasterDbContext` vs `TenantDbContext`) picks up the entity at startup.

## Master database

```mermaid
erDiagram
    TENANT ||--o| SUBSCRIPTION : has
    TENANT ||--o{ USER : "scopes (TenantId)"
    TENANT ||--o{ TENANT_FEATURE_CONFIG : configures
    TENANT ||--o{ API_KEY : "issues (MCP)"
    TENANT ||--o{ USER_TENANT_ACCESS : "shares with users"

    SUBSCRIPTION }o--|| SUBSCRIPTION_PLAN : on
    SUBSCRIPTION_PLAN ||--o{ PLAN_FEATURE : grants

    USER ||--o| TENANT : "belongs to (optional)"

    SUBSCRIPTION_INVOICE }o--|| TENANT : "billed to"
    SUBSCRIPTION_INVOICE }o--|| SUBSCRIPTION : "for"

    TENANT {
        guid Id
        string Name "slug, lowercase"
        string ConnectionString "tenant DB"
        Address CompanyAddress
        bool IsSubscriptionRequired
        TenantSettings Settings
        StripeConnectStatus ConnectStatus
        string StripeConnectedAccountId
    }
    SUBSCRIPTION {
        guid Id
        SubscriptionStatus Status
        guid TenantId
        guid PlanId
        string StripeSubscriptionId
        bool CancelAtPeriodEnd
    }
    SUBSCRIPTION_PLAN {
        guid Id
        string Name
        PlanTier Tier
        Money BaseFee
        Money PerTruckFee
        LlmModelTier AllowedModelTier
        int WeeklyAiRequestQuota
    }
    USER {
        guid Id
        string Email
        string FirstName
        string LastName
        guid TenantId "nullable"
    }
```

The master DB also stores `SuperAdmin`, `BlogPost`, `ContactSubmission`, `DemoRequest`, `SystemSetting`, `DefaultFeatureConfig`, and `ImpersonationAuditLog` / `ImpersonationToken` (used by SuperAdmins to act on behalf of a tenant).

### Tenants and subscriptions

Every customer company is one `Tenant` row. The connection string for that company's tenant DB is stored in `Tenant.ConnectionString`, which is generated from `TenantDatabaseDefaults.NameTemplate` at provisioning time (see [Multi-Tenancy](multi-tenancy.md)).

Subscriptions are 1:1 with tenants and reference a `SubscriptionPlan`. Plan tiers (Starter / Professional / Enterprise) gate features through the `PlanFeature` join table and cap LLM model access via `AllowedModelTier`.

## Tenant database

A tenant DB holds the entire operational graph for one company. The most important aggregates are **Load**, **Trip**, **Truck**, **Customer**, **Container**, and **Invoice**.

```mermaid
erDiagram
    CUSTOMER ||--o{ LOAD : "books"
    LOAD }o--|| LOAD_INVOICE : "billed via"
    LOAD }o--o| TRUCK : "assigned to"
    LOAD }o--o| EMPLOYEE : "dispatched by"
    LOAD }o--o| CONTAINER : "moves"
    LOAD }o--o| TERMINAL : "origin"
    LOAD }o--o| TERMINAL : "destination"
    LOAD ||--o{ LOAD_DOCUMENT : has
    LOAD ||--o{ LOAD_EXCEPTION : raises

    TRIP }o--o| TRUCK : "performed by"
    TRIP ||--|{ TRIP_STOP : "ordered stops"
    TRIP_STOP }o--|| LOAD : "for"

    TRUCK }o--o| EMPLOYEE : "main driver"
    TRUCK }o--o| EMPLOYEE : "secondary driver"
    TRUCK ||--o{ TRUCK_DOCUMENT : has

    EMPLOYEE }o--o| TENANT_ROLE : "has role"
    EMPLOYEE ||--o{ PAYROLL_INVOICE : "paid via"
    EMPLOYEE ||--o{ TIME_ENTRY : tracks

    LOAD_INVOICE ||--o{ INVOICE_LINE_ITEM : contains
    LOAD_INVOICE ||--o{ PAYMENT : "settled by"
    LOAD_INVOICE ||--o{ PAYMENT_LINK : "shareable link"

    CONTAINER }o--o| TERMINAL : "currently at"

    DISPATCH_SESSION ||--o{ DISPATCH_DECISION : produces
    DISPATCH_DECISION }o--o| LOAD : "targets"
    DISPATCH_DECISION }o--o| TRUCK : "targets"
    DISPATCH_DECISION }o--o| TRIP : "targets"

    CUSTOMER {
        guid Id
        string Name
        CustomerStatus Status
    }
    LOAD {
        long Number
        string Name
        LoadType Type
        LoadStatus Status
        Address OriginAddress
        Address DestinationAddress
        Money DeliveryCost
        double Distance
        LoadSource Source
        DateTime RequestedPickupDate "nullable"
        DateTime RequestedDeliveryDate "nullable"
    }
    TRIP {
        long Number
        string Name
        TripStatus Status
        double TotalDistance
        DateTime DispatchedAt "nullable"
        DateTime CompletedAt "nullable"
    }
    TRIP_STOP {
        TripStopType Type
        int Order "1-based"
        Address Address
        DateTime ArrivedAt "nullable"
    }
    TRUCK {
        string Number
        TruckType Type
        TruckStatus Status
        string Vin
        string Make
        string Model
        int VehicleCapacity
    }
    EMPLOYEE {
        string Email
        string FirstName
        string LastName
        Money Salary
        SalaryType SalaryType
        EmployeeStatus Status
    }
    CONTAINER {
        string Number "ISO 6346"
        ContainerIsoType IsoType
        ContainerStatus Status
        bool IsLaden
        decimal GrossWeight
    }
    TERMINAL {
        string Name
        string Code "UN/LOCODE"
        string CountryCode "ISO 3166-1"
        TerminalType Type
    }
```

A tenant DB also includes the supporting graph: **HOS / ELD** (`HosLog`, `HosViolation`, `DriverHosStatus`, `EldDriverMapping`, `EldVehicleMapping`, `EldProviderConfiguration`), **Safety** (`DvirReport`, `DvirDefect`, `AccidentReport`, `DriverBehaviorEvent`), **Maintenance** (`MaintenanceSchedule`, `MaintenanceRecord`, `MaintenancePart`), **Expenses** (`CompanyExpense`, `TruckExpense`, `BodyShopExpense`), **Messaging** (`Conversation`, `Message`, `MessageReadReceipt`), **Load board** (`LoadBoardConfiguration`, `LoadBoardListing`, `PostedTruck`), and **Notifications** / **Tracking links**.

## Aggregates

An aggregate is a consistency boundary - the root entity is the only thing outside code can hold a reference to, and operations on the aggregate go through the root.

| Aggregate root    | Owns                                                                        |
| ----------------- | --------------------------------------------------------------------------- |
| `Tenant`          | `Subscription`, `TenantFeatureConfig[]`                                     |
| `Load`            | `LoadDocument[]`, `LoadException[]`, optional `LoadInvoice`                 |
| `Trip`            | `TripStop[]` (each stop references a `Load` but stops are part of the trip) |
| `Truck`           | `TruckDocument[]`                                                           |
| `Container`       | (lifecycle owner - referenced by `Load`)                                    |
| `Invoice` TPH     | `InvoiceLineItem[]`, `Payment[]`, `PaymentLink[]`                           |
| `DispatchSession` | `DispatchDecision[]`                                                        |
| `Conversation`    | `Message[]`, `ConversationParticipant[]`, `MessageReadReceipt[]`            |
| `DvirReport`      | `DvirDefect[]`                                                              |
| `AccidentReport`  | `AccidentThirdParty[]`, `AccidentWitness[]`                                 |

Cross-aggregate references are by `Guid` ID, not navigation property, with one important exception: EF Core lazy loading is enabled, so `virtual` navigation properties are populated on access. The codebase relies on this and avoids `.Include()` calls.

## Lifecycles (state machines)

Status transitions are enforced inside the entity, not in handlers. Each one has a dedicated `*StatusMachine` static class that decides whether a transition is legal.

### Load

```mermaid
stateDiagram-v2
    [*] --> Draft: create / quote / book
    Draft --> Dispatched: Dispatch()
    Dispatched --> PickedUp: ConfirmPickup()
    PickedUp --> Delivered: ConfirmDelivery()
    Draft --> Cancelled: Cancel()
    Dispatched --> Cancelled: Cancel()
    PickedUp --> Cancelled: Cancel()
    Delivered --> [*]
    Cancelled --> [*]
```

`Load.Dispatch()` also flips a draft `Invoice` to `Issued`. `Load.UpdateProximity(true)` raises `LoadProximityChangedEvent` when the truck enters the geofence of the next checkpoint, enabling the driver-app pickup/delivery confirm button.

### Trip

```mermaid
stateDiagram-v2
    [*] --> Draft: assign loads
    Draft --> Dispatched: Dispatch()
    Dispatched --> InTransit: any pickup confirmed
    InTransit --> Completed: all drop-offs delivered
    Draft --> Cancelled: Cancel()
    Dispatched --> Cancelled: Cancel()
    InTransit --> Cancelled: Cancel()
    Completed --> [*]
    Cancelled --> [*]
```

`Trip.MarkStopArrived(stopId)` propagates the per-stop arrival to the underlying `Load` (via `force: true`) and refreshes the trip's status (`Dispatched` → `InTransit` → `Completed`). Cancelling a trip cascades a `Load.Cancel()` to every stop's load.

### Container

```mermaid
stateDiagram-v2
    [*] --> Empty
    Empty --> Loaded: MarkAsLoaded()
    Loaded --> AtPort: MarkAtPort(terminal)
    Empty --> AtPort: MarkAtPort(terminal)
    AtPort --> InTransit: MarkInTransit()
    Loaded --> InTransit: MarkInTransit()
    InTransit --> Delivered: MarkDelivered()
    Delivered --> Returned: MarkReturned(depot)
    Returned --> Empty: MarkAsEmpty()
    Returned --> [*]
```

Every transition raises `ContainerStatusChangedEvent`. `MoveToTerminal(terminal)` is a _pure location update_ that does not change the status (used when a container is shuffled inside a yard).

### Invoice

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> PendingApproval
    PendingApproval --> Approved
    PendingApproval --> Rejected
    Approved --> Issued
    Rejected --> Draft
    Issued --> Sent: email/PDF delivered
    Sent --> PartiallyPaid: payment < total
    Sent --> Paid: payment >= total
    PartiallyPaid --> Paid
    Issued --> Overdue: due date passed
    Sent --> Overdue
    Draft --> Cancelled
    Issued --> Cancelled
    Paid --> [*]
    Cancelled --> [*]
```

`Invoice.ApplyPayment(payment)` adds the payment, sums all payments, and flips status to `Paid` or `PartiallyPaid` based on the total.

### Dispatch session

```mermaid
stateDiagram-v2
    [*] --> Running: agent starts
    Running --> Completed: Complete(summary)
    Running --> Failed: Fail(errorMessage)
    Running --> Cancelled: Cancel()
    Completed --> [*]
    Failed --> [*]
    Cancelled --> [*]
```

A session captures token usage (`InputTokensUsed`, `OutputTokensUsed`, `CacheReadTokens`, `CacheCreationTokens`), estimated USD cost, and the `RequestCost` multiplier (1 / 5 / 10) used by the AI quota system.

## Invoice hierarchy (TPH)

`Invoice` is an abstract base mapped Table-Per-Hierarchy. The discriminator is `InvoiceType`.

```mermaid
classDiagram
    class Invoice {
        <<abstract>>
        +long Number
        +InvoiceStatus Status
        +Money Total
        +DateTime? DueDate
        +string? StripeInvoiceId
        +List~InvoiceLineItem~ LineItems
        +List~Payment~ Payments
        +List~PaymentLink~ PaymentLinks
        +ApplyPayment(payment)
    }

    class LoadInvoice {
        +Guid CustomerId
        +Customer Customer
        +Guid LoadId
        +Load Load
    }

    class PayrollInvoice {
        +Guid EmployeeId
        +Employee Employee
        +DateTime PeriodStart
        +DateTime PeriodEnd
    }

    class SubscriptionInvoice {
        +Guid TenantId
        +Tenant Tenant
        +Guid SubscriptionId
    }

    Invoice <|-- LoadInvoice
    Invoice <|-- PayrollInvoice
    Invoice <|-- SubscriptionInvoice
```

`LoadInvoice` and `PayrollInvoice` live in the **tenant DB**; `SubscriptionInvoice` lives in the **master DB** (platform billing). The base class implements both `IMasterEntity` and `ITenantEntity` so each subclass can be picked up by the right `DbContext`.

## Value objects

| Value object     | Used by                                            | Notes                                                           |
| ---------------- | -------------------------------------------------- | --------------------------------------------------------------- |
| `Address`        | Tenant, Customer, Load, Trip stop, Terminal, Truck | Line1/Line2, City, State, ZipCode, Country (ISO 3166-1 alpha-2) |
| `GeoPoint`       | Load (origin/destination), TripStop, Truck         | `Latitude`, `Longitude`                                         |
| `Money`          | Load, Invoice, Employee, SubscriptionPlan          | `Amount` + `Currency`. Mapped as a complex property             |
| `LoadRoute`      | Routing service                                    | Computed origin/destination + distance                          |
| `Page` / `Sort`  | Repositories / queries                             | Pagination + sort spec                                          |
| `TenantSettings` | Tenant                                             | Region, currency, units, AI provider/model, locale              |

`Money` and `Address` are mapped as **complex properties** (EF Core 8+) - they have no primary key and are inlined in the owner's table.

## Domain events

Entities raise events from their methods. Events are dispatched via MediatR after `SaveChanges` succeeds, so a failed transaction never publishes its events.

```mermaid
sequenceDiagram
    autonumber
    participant Cmd as Command Handler
    participant Agg as Aggregate Root
    participant UoW as UnitOfWork
    participant Db as DbContext
    participant Mediator
    participant Handler as Event Handler

    Cmd->>Agg: aggregate.DoSomething()
    Agg->>Agg: AddDomainEvent(...)
    Cmd->>UoW: SaveChangesAsync()
    UoW->>Db: persist
    Db-->>UoW: ok
    UoW->>Mediator: Publish each event
    Mediator->>Handler: Handle(event)
```

A non-exhaustive list of domain events:

- `LoadCompletedEvent`, `LoadCancelledEvent`, `LoadProximityChangedEvent`
- `TripDispatchedEvent`, `TripCompletedEvent`
- `ContainerStatusChangedEvent`
- `PayrollInvoice` events (created, approved, paid)

Handlers live in `src/Core/Logistics.Application/Events/` and produce side effects: notifications, follow-on jobs, audit log entries, etc.

## Specifications

`ISpecification<T>` encapsulates reusable query conditions used by `IRepository<T>`. They are defined in `src/Core/Logistics.Domain/Specifications/`.

```csharp
public class ActiveLoadsSpec : Specification<Load>
{
    public ActiveLoadsSpec()
        => Query.Where(l => l.Status != LoadStatus.Cancelled
                         && l.Status != LoadStatus.Delivered)
                .OrderByDescending(l => l.CreatedAt);
}

// Used by a query handler:
var activeLoads = await tenantUow.Repository<Load>().GetListAsync(new ActiveLoadsSpec(), ct);
```

Specifications keep query intent in the domain layer rather than scattered across handlers.

## Auditing

Every `AuditableEntity` gets four fields filled in automatically by an EF Core `SaveChanges` interceptor in `Logistics.Infrastructure.Persistence`:

- `CreatedAt`, `CreatedBy` - set on insert from the current user (or `system` for background jobs).
- `UpdatedAt`, `UpdatedBy` - set on every update.

Non-auditable entities (e.g. `Truck`, `Employee`, `User`) inherit only `Entity` - they have an ID but no audit columns. Lookup-style entities like `Terminal`, `Container`, `Invoice` are auditable because operations history is regulatory-relevant (custody chain, billing).

## Next Steps

- [Architecture Overview](overview.md) - layer split and infrastructure projects
- [Multi-Tenancy](multi-tenancy.md) - master vs tenant DB resolution
- [API Overview](../api/overview.md) - REST endpoints by aggregate
