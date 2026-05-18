# Application Module Layout

`Logistics.Application` is organised into six modules. Each module owns the features inside one bounded context — pick a module by context, not by entity.

## The six modules

| Module           | Bounded context                              | Example features                                                                           |
| ---------------- | -------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `Operations`     | Daily TMS work: dispatching, fleet, tracking | Loads, Trips, Trucks, Containers, Terminals, Tracking                                      |
| `Compliance`     | Regulatory and safety                        | ELD/HOS, DVIR, Inspections, Accidents, Safety, Privacy                                     |
| `Financial`      | Money in / money out                         | Invoices, Tax, Payments, PaymentLinks, Payroll, StripeConnect, Expenses                    |
| `IdentityAccess` | Who can do what, and which org               | Users, Roles, Tenants, Customers, Employees, Subscriptions, ApiKeys, Features, Invitations |
| `Integrations`   | Outbound integrations + inbound webhooks     | AiDispatch, LoadBoard, Webhooks, Messaging, Documents, UpdateNotifications                 |
| `Platform`       | Cross-cutting, marketing, internal           | Stats, Reports, BlogPosts, Contacts, DemoRequests, Notifications                           |

## Folder convention inside a module

```text
Modules/{Module}/
├── {Module}ModuleRegistrar.cs          # DI extensions for this module
└── {Feature}/                          # One folder per top-level feature
    ├── Commands/
    │   └── {Verb}{Entity}/             # One folder per command
    │       ├── {Verb}{Entity}Command.cs
    │       ├── {Verb}{Entity}Handler.cs
    │       └── {Verb}{Entity}Validator.cs   # optional
    ├── Queries/
    │   └── Get{Entity}ById/
    │       ├── Get{Entity}ByIdQuery.cs
    │       └── Get{Entity}ByIdHandler.cs
    ├── Events/                         # optional: domain-event handlers
    ├── Policies/                       # optional: authorisation / domain policies
    └── ReadModels/                     # optional: projected read models
```

### Verified example

```text
Modules/Compliance/Accidents/
├── Commands/
│   └── CreateAccidentReport/
│       ├── CreateAccidentReportCommand.cs
│       └── CreateAccidentReportHandler.cs
└── Queries/
```

See `src/Core/Logistics.Application/Modules/Compliance/Accidents/Commands/CreateAccidentReport/CreateAccidentReportCommand.cs`.

## Picking the right module for a new feature

Use bounded context, not entity affinity:

- A _driver licence_ is owned by an Employee, but checking expiry is **Compliance** behaviour → `Modules/Compliance/...`. If the licence record itself lives next to the employee, that part stays in `Modules/IdentityAccess/Employees/...`.
- A _Stripe webhook_ could feel like Financial, but webhook plumbing is **Integrations** → `Modules/Integrations/Webhooks/`. The downstream effect (issuing an invoice) is Financial — split the command from the side effect.
- _Blog posts_ power the marketing site, so they're **Platform**, not IdentityAccess, even though they have an Author.

When two modules both look right, pick the one whose other features the new command will mostly call.

## Module registrar

Every module ships a `{Module}ModuleRegistrar.cs` that exposes a single `Add{Module}Module(IServiceCollection)` extension. Most are empty today — MediatR handlers, FluentValidation validators, and `IApplicationService` implementations are registered assembly-wide by `Registrar.AddApplicationCommon` / `AddApplicationServices` in `Logistics.Application/Registrar.cs`. Only put a service in a module registrar when the global scan can't cover it (decorators, named instances, keyed services, factories).

Composition roots (`Program.cs` in each Presentation project) call `AddApplicationCommon`, `AddApplicationServices`, then each `Add{Module}Module()` in turn.

## Scaffolding a new feature

Use the [scaffold-feature](../../.claude/skills/scaffold-feature/SKILL.md) skill — it writes files to the correct module path, names everything consistently, and updates `.claude/feature-map.md` for you.
