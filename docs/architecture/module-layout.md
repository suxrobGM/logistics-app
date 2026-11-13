# Application Module Layout

`Logistics.Application` is organised into nine modules. Each module owns the features inside one bounded context - pick a module by context, not by entity. Code shared across modules lives in `Logistics.Application/Common/`, which is not a module. The same module names group the API controllers, the Domain entity and enum folders, and the EF configurations, so a feature sits under the same module in every project.

## The nine modules

| Module         | Bounded context                                   | Features                                                                                         |
| -------------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `Operations`   | Daily TMS work: dispatching, fleet, shippers      | Loads, Trips, Trucks, Trailers, Containers, Terminals, Tracking, Maintenance, TimeEntries, Customers, Documents, Messaging, Notifications, Portal, DataImport |
| `Compliance`   | Regulatory and safety                             | Eld, Dvir, Inspections, Safety, Privacy, Ifta                                                    |
| `Financial`    | Money in / money out                              | Invoices, Tax, Payments, PaymentLinks, Payroll, StripeConnect, Expenses, Factoring, Accessorials |
| `Identity`     | Who can sign in and what they may do              | Users, Roles, Employees, Invitations, ApiKeys, Admins, CustomerUsers                             |
| `Tenancy`      | A tenant's lifecycle and what its plan unlocks    | Tenants, Subscriptions, Features, ProductLicense, Onboarding                                     |
| `AI`           | Agent-driven work and its settings                | AIDispatch, AICopilot, Agents, Negotiation, AISettings                                           |
| `Integrations` | Outside vendors and inbound webhooks              | LoadBoard, FuelCards, Accounting, Webhooks                                                       |
| `Reporting`    | Read-only reports and dashboards over tenant data | Reports, Stats                                                                                   |
| `Website`      | The marketing site                                | BlogPosts, Contacts, DemoRequests                                                                |

## Folder convention inside a module

```text
Modules/{Module}/
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
    ├── Services/                       # optional: feature-scoped workflow services
    ├── Constants/                      # optional
    └── {Entity}Mapper.cs               # Mapperly mapper for the feature's entities
```

### Verified example

```text
Modules/Compliance/Safety/
├── Commands/
│   └── CreateAccidentReport/
│       ├── CreateAccidentReportCommand.cs
│       └── CreateAccidentReportHandler.cs
├── Queries/
├── Services/
└── AccidentMapper.cs
```

See `src/Core/Logistics.Application/Modules/Compliance/Safety/Commands/CreateAccidentReport/CreateAccidentReportCommand.cs`.

## Picking the right module for a new feature

Use bounded context, not entity affinity:

- A _driver licence_ is owned by an Employee, but checking expiry is **Compliance** behaviour → `Modules/Compliance/Safety/`. The licence record itself stays next to the employee in `Modules/Identity/Employees/`.
- A _Stripe webhook_ could feel like Financial, but webhook plumbing is **Integrations** → `Modules/Integrations/Webhooks/`. The downstream effect (issuing an invoice) is Financial - split the command from the side effect.
- _Broker negotiation_ talks to outside brokers, but an agent drives it, so it is **AI**, not Integrations.
- _Blog posts_ power the marketing site, so they're **Website**, not Identity, even though they have an Author.

When two modules both look right, pick the one whose other features the new command will mostly call.

## Service registration

Modules have no registrars of their own. Request handlers, FluentValidation validators, and `IApplicationService` implementations are registered assembly-wide by the private `AddApplicationCommon` / `AddApplicationServices` scans in `Logistics.Application/Registrar.cs`. A service the scan can't cover (decorators, named instances, keyed services, factories) is registered explicitly in `AddApplicationLayer`.

The whole layer is wired by the single aggregate `services.AddApplicationLayer()`, which runs `AddApplicationCommon` and `AddApplicationServices`, then the few explicit registrations. The **API** is the only host that calls it - from `Logistics.API/Setup.cs` (`ConfigureServices`), reached through the thin `LogisticsHost.Run` shell in `Program.cs`. IdentityServer and DbMigrator do **not** call `AddApplicationLayer`; DbMigrator pulls in only the narrow slices it needs for seeding (`AddApplicationTaxServices`, `AddApplicationFuelCardServices`).

## Scaffolding a new feature

Use the [scaffold-feature](../../.claude/skills/scaffold-feature/SKILL.md) skill - it writes files to the correct module path, names everything consistently, and updates `.claude/feature-map.md` for you.
