# Feature Map

Stable starting points for finding code by feature. Read this before grepping. If a feature isn't here, it doesn't exist yet (or this file is stale - flag it).

Each feature lists only the layers it touches: **Domain** (entities/VOs), **Application** (commands/queries/services), **Infrastructure** (service implementations), **API/UI** (controllers, jobs, frontend pages). A missing label means that layer has nothing feature-specific.

This file answers _where_. For _how it works_, follow the deep dive: **AI dispatch** → [docs/ai-dispatch.md](../docs/ai-dispatch.md) · **Financial** → [invoices](../docs/invoices.md), [customer payments](../docs/customer-payments.md), [Stripe Connect](../docs/stripe-connect.md), [Stripe webhooks](../docs/stripe-webhooks.md) · **MCP** → [docs/mcp-server.md](../docs/mcp-server.md) · **architecture** → [overview](../docs/architecture/overview.md), [multi-tenancy](../docs/architecture/multi-tenancy.md), [domain model](../docs/architecture/domain-model.md), [module layout](../docs/architecture/module-layout.md) · **not built yet** → [docs/roadmap/](../docs/roadmap/README.md).

## Where things go (conventions)

- **Domain entity**: `src/Core/Logistics.Domain/Entities/{Feature}/`
- **Commands**: `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Commands/{Verb}{Entity}/`
- **Queries**: `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Queries/Get{Entity}/`
- **Domain events**: `src/Core/Logistics.Domain/Events/`; handlers in `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Events/`
- **Domain specifications** (base classes only): `src/Core/Logistics.Domain/Specifications/`
- **Application specifications** (feature-scoped, e.g. `FilterLoadInvoices`, `FilterLoadsByDeliveryDate`): `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Specifications/`
- **Infrastructure-port interfaces** (implemented by Infrastructure): `src/Core/Logistics.Application.Abstractions/{Domain}/`
- **Application workflow services** (stay in Application): `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Services/`
- **Application constants**: feature-scoped under `Modules/{Module}/{Feature}/Constants/`; cross-module shared in `Modules/Common/Constants/`
- **Service implementations**: `src/Infrastructure/Logistics.Infrastructure.{Module}/`
- **Shared provider HTTP**: `Logistics.Infrastructure.Integrations.Common/HttpClientJsonExtensions.cs` - `TryGetFromJsonAsync` (never throws; logs, returns `default`), used by FuelCards/Eld (load boards throw instead: `Integrations.LoadBoard/LoadBoardHttp.cs`). Don't copy it into a new integration. `Integrations.Accounting`'s QBO helpers deliberately **throw** so push errors surface - not duplicates, don't fold in
- **REST controllers**: `src/Presentation/Logistics.API/Controllers/{Feature}Controller.cs`
- **SignalR hubs**: `src/Infrastructure/Logistics.Infrastructure.Communications/SignalR/Hubs/`
- **Hangfire jobs**: `src/Presentation/Logistics.API/Jobs/` - fan out with `TenantJobRunner.ForEachTenantAsync` and gate features inside the body (see CLAUDE.md for the canonical `[RequiresFeature]`-is-inert rule)
- **Machine-readable API errors**: `Logistics.Shared.Models/ErrorCodes.cs`, mirrored in `projects/shared/src/lib/errors/upgrade-handler.ts` (see `.claude/rules/backend/api-design.md` for the contract)
- **Webhooks**: `WebhookController.cs` + `Application/Modules/Integrations/Webhooks/Commands/`
- **EF configurations**: `src/Infrastructure/Logistics.Infrastructure.Persistence/Configurations/{Feature}/`
- **Frontend pages**: `private/src/Client/Logistics.Angular/projects/{portal}/src/app/pages/{feature}/`
  - Portals: `tms-portal` (dispatchers), `customer-portal` (shippers), `admin-portal` (super admin), `website` (marketing)
- **Mobile (driver)**: `private/src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/`
- **Provider connect dialogs**: `tms-portal/src/app/shared/components/integrations/provider-connect-dialog/` - MUST be used for every ELD / load board / fuel card "add provider" dialog (see the Angular `CLAUDE.md` for how)
- **Address inputs**: `<ui-address-form>` from `@logistics/shared/ui` - MUST be used by every form that captures an `Address` value object; never raw `<input>`s. Country drives the State / Region / Province label (`state-labels.ts`), State stays required everywhere. Server-side counterpart: `Logistics.Application.Validators.AddressValidator`

## Operations

### Loads

- Domain: `Entities/Load/`
- Application: `Modules/Operations/Loads/` (Commands, Queries, Services, Specifications, Constants)
- API/UI: `LoadController.cs`, `tms-portal/pages/loads/`
- Car hauler vehicles: `Entities/Load/LoadVehicle.cs` + `Load.Vehicles.cs` (JSON complex collection `loads.vehicles`; `LoadVehicle.Id` is what a condition report points at), input `LoadVehicleInput` + `Validators/LoadVehicleInputValidator.cs`, `Services/LoadVehicleInputExtensions.cs`. Capacity warning: `Operations/Common/Services/VehicleCapacityChecker.cs` (used by `GetLoadByIdHandler` and `DispatchEligibilityService` as `VehicleCapacityExceeded`, a warning). eBOL: `Queries/GetVehicleBolPdf/` → `GET /loads/{id}/ebol`, `GET /portal/loads/{id}/ebol`; delivery email `Services/VehicleDeliveryEmailService.cs` (from `ConfirmLoadStatusHandler`, template `VehicleDelivered.liquid`). UI: `load-form/load-vehicles-section.ts` (per-row VIN decode, capacity warning), `pages/loads/components/load-vehicles-content/` (details + eBOL download), `shared/components/display/vehicle-damage-diagram/` (read-only diagram on condition-report-details), customer-portal `shipments/shipment-details` + `documents/`. Guide: [docs/car-hauler-guide.md](../docs/car-hauler-guide.md)
- Charges and LTL: `Load.Billing.cs` (`FuelSurcharge`, `Accessorials`), `Load.Freight.cs` (freight class, NMFC, pallets, pieces, weight, dimensions, stackable), `LoadAccessorial.cs`; invoice lines from `Entities/Invoice/LoadInvoice.Charges.cs` (`ReplaceLoadCharges` - owns `BaseRate`, `FuelSurcharge` and lines whose `SourceId` is an accessorial). Application: `Services/LoadChargeService.cs` (replace accessorials, sync the invoice, write detention), `Commands/{Create,Update}Load/*Command.Charges.cs`. UI: `load-form/load-charges-section.ts`, `load-form/load-freight-section.ts`, `pages/loads/components/load-charges-content/`
- Detention: `Entities/Trip/DetentionCalculator.cs` + `DetentionTerms.cs` (customer → tenant → 2 h), applied by `MarkStopDepartedHandler` → `LoadChargeService.ApplyDetentionAsync`; `Queries/GetLoadDetention/` (`GET /loads/{id}/detention`, UI `pages/loads/components/load-detention-content/`); alert `Modules/Operations/Trips/Services/DetentionAlertService.cs` + `Jobs/DetentionAlertJob.cs` (every 15 min, once per stop via `TripStop.DetentionAlertedAt`)

### Trips

- Domain: `Entities/Trip/`
- Application: `Modules/Operations/Trips/Commands/`, `Modules/Operations/Trips/Queries/`
- Infrastructure: `Routing/Optimization/` (trip optimizer)
- API/UI: `TripController.cs`, `tms-portal/pages/trips/`; driver stop events `DriverController.cs` (`POST /drivers/trips/{tripId}/stops/{stopId}/arrive|depart`, assignment-checked, sets `TripStop.ArrivedAt`/`DepartedAt`) - departure also bills detention (see Loads)

### Trucks

- Domain: `Entities/Truck.cs`
- Application: `Modules/Operations/Trucks/Commands/`, `Modules/Operations/Trucks/Queries/`
- Truck/load type pairings: `Domain.Primitives/Enums/Truck/TruckLoadCompatibility.cs` (car haulers carry only vehicles, container trucks only containers) - enforced by `AssignLoadToTruckHandler` and `DispatchEligibilityService` (`TruckTypeIncompatible`), and rendered into both agent prompts by `Infrastructure.AI/Agents/PromptText.cs`
- API/UI: `TruckController.cs`, `tms-portal/pages/trucks/`

### Trailers

- Domain: `Entities/Trailer.cs`, `Primitives/Enums/Trailer/` (`TrailerType` incl. `Chassis`, `TrailerStatus`); `Truck.CurrentTrailerId` (hooked trailer, filtered unique index), `Trip.TrailerId` (trailer pulled on the trip; loads read it through their trip as `LoadDto.TrailerNumber`, filled by `GetLoadById` only). `LengthMeters` is always metres
- Application: `Modules/Operations/Trailers/Commands/`, `Modules/Operations/Trailers/Queries/`; hooking goes through `Modules/Operations/Trailers/Services/TrailerHookService` - `HookAsync` (from `UpdateTruckCommand.CurrentTrailerId`; moves a trailer off another truck) and `UnhookUnlessActiveAsync` (from `UpdateTrailer` and the data-import `TrailerImporter`, when a trailer leaves Active); `CreateTrip` defaults to the truck's trailer, `UpdateTrip` takes the new truck's trailer when the truck changes
- API/UI: `TrailerController.cs` (`Permission.Trailer.*`), `tms-portal/pages/trailers/`, current-trailer picker in `pages/trucks/components/truck-form/`, trailer picker in `pages/trips/components/trip-wizard-basic/`, driver app `feature/trips/TripDetailScreen.kt`
- Seed: `DbMigrator/Seeders/FakeData/TrailerSeeder.cs` (one per truck plus spares)
- Not built: trailer documents, trailer DVIR, load-board posting by trailer type

### Customers

- Domain: `Entities/Customer.cs`
- Application: `Modules/IdentityAccess/Customers/Commands/`, `Modules/IdentityAccess/Customers/Queries/`
- API/UI: `CustomerController.cs`, `tms-portal/pages/customers/`

### Customer users

- Domain: `Entities/Customer/CustomerUser.cs`
- Application: `Modules/IdentityAccess/CustomerUsers/Commands/`
- API/UI: `CustomerUserController.cs`, `customer-portal/pages/`

### Customer portal

- Application: `Modules/Platform/Portal/Queries/` (portal loads, load documents + download, invoices, customer users, tenant access)
- API/UI: `PortalController.cs` (`/portal/*`), `customer-portal/pages/` (`shipments/`, `documents/`, `invoices/`, `tracking/`, `payment/`, `account/`)

### Containers (ISO 6346)

- Domain: `Entities/Container/` - `Number` is canonicalised on write by `Container.NormalizeNumber`
- Application: `Modules/Operations/Containers/Commands/`, `Modules/Operations/Containers/Queries/`
- API/UI: `ContainerController.cs`, `tms-portal/pages/containers/`
- Feature flag: `TenantFeature.IntermodalContainers` (Professional+; off by default)
- Drayage (per move, on the container): `Container.Drayage.cs` (steamship line, LFD / empty return deadline + daily rates, holds, appointments, chassis number/pool/rate, empty return terminal, street turn), clocks in `ContainerFreeTimeCalculator` (calendar days, UTC date, deadline day free). `PickedUpAt` is stamped when status leaves `AtPort`; leaving `Returned` clears the per-move data (`ContainerStatusMachine`)
- Drayage commands: `UpdateContainerDrayage` (`PUT /containers/{id}/drayage`, full replace), `StreetTurnContainer` (`POST /containers/{id}/streetturn`), `AddDrayageCharges` (`POST /loads/{id}/drayagecharges`, writes Demurrage/PerDiem/Chassis per-day accessorials, replacing its own, then syncs the invoice)
- Job: `ContainerDeadlineAlertJob` (daily 12:00 UTC) -> `Containers/Services/ContainerDeadlineAlertService` (`INotificationService`)
- UI: `tms-portal/shared/components/drayage/container-drayage-panel/` (container + load details), `tags/free-time-tag`, `tags/container-holds-tag`, `pages/containers/components/drayage-form/` (container edit)

### Terminals (UN/LOCODE)

- Domain: `Entities/Terminal/` - `Code` is canonicalised on write by `Terminal.NormalizeCode`
- Application: `Modules/Operations/Terminals/Commands/`, `Modules/Operations/Terminals/Queries/`
- API/UI: `TerminalController.cs`, `tms-portal/pages/terminals/`
- Feature flag: `TenantFeature.IntermodalContainers` (shared with Containers)

### Employees / Drivers

- Domain: `Entities/Employee.cs`
- Application: `Modules/IdentityAccess/Employees/Commands/`, `Modules/IdentityAccess/Employees/Queries/`
- API/UI: `EmployeeController.cs`, `DriverController.cs`, `tms-portal/pages/employees/`

### Time entries

- Domain: `Entities/TimeEntry.cs`
- Application: `Modules/Operations/TimeEntries/Commands/`, `Modules/Operations/TimeEntries/Queries/`
- API/UI: `TimeEntryController.cs`, `tms-portal/pages/timesheets/`

### Dashboard / stats

- Application: `Modules/Platform/Stats/Queries/`, `Modules/Platform/Reports/Queries/`
- API/UI: `StatController.cs`, `ReportController.cs`, `tms-portal/pages/home/` (+ its `components/` panels), `pages/reports/`. `pages/dashboard/` is a redirect-only stub kept for old bookmarks - don't add anything there

### Onboarding

- Application: `Modules/Platform/Onboarding/Queries/GetOnboardingProgress/` - read-only checklist; step keys are camelCase, labels/icons/routes live on the client. Each step declares a `TenantFeature`, so a disabled feature drops its step rather than shipping one that can never complete
- API/UI: `OnboardingController.cs` (`GET /onboarding/progress`, `Permission.Employee.Manage` - Owner + Manager), `tms-portal/pages/home/components/onboarding-checklist/` (also links to the data import)

### Data import (CSV / XLSX from another TMS)

Stateless: the client re-sends the file on every step; nothing is stored. Full guide: [docs/data-import.md](../docs/data-import.md).

- Application: `Modules/Platform/DataImport/` - `Constants/ImportFieldCatalog.cs` (fields + header synonyms per entity), `Services/` (`ImportHeaderMapper`, `ImportValueParser`, `ImportAddressParser`), `Importers/` (one `EntityImporter<TDraft>` per entity: customers, drivers, trucks, trailers, loads), `Commands/RunImport/` (validate = dry run, import), `Queries/PreviewImport|GetImportTemplate/`
- Infrastructure: `Infrastructure.Documents/Spreadsheets/SpreadsheetReader.cs` (ExcelDataReader) behind `Application.Abstractions/Documents/ISpreadsheetReader`
- API/UI: `ImportController.cs` (`/imports/*`, `Permission.Import.Manage`), `tms-portal/pages/settings/data-import/`
- Trap: imported drivers get a password-less master `User`; `CreateInvitationHandler` lets such a member be invited and `AcceptInvitationHandler` sets the password on accept instead of failing "already an employee"

### Tenant presets and solo mode

- Domain: `Primitives/Enums/Tenant/TenantPreset.cs` (`GeneralFreight` | `CarHauler` | `Intermodal` | `SoloOperator`), stored as `Tenant.Presets` (`presets text[]` on `tenants`, master DB) with `Tenant.IsSolo`; `Entities/Feature/TenantPresetCatalog.cs` maps presets to features (presets combine; solo drops `Payroll`/`Timesheets`)
- Application: `Modules/IdentityAccess/Tenants/Commands/CreateTenant` + `UpdateTenantPresets` (admin roles only) call `IFeatureService.ApplyPresetFeaturesAsync`; `Modules/Platform/Onboarding/` drops the `inviteTeam` step for solo; `Modules/Operations/Common/Services/VehicleTransportGuard.cs` gates vehicle load and car hauler truck types on `TenantFeature.VehicleTransport`
- Infrastructure: `Infrastructure.AI/Agents/Dispatch/AIDispatchSystemPrompt.cs` - solo swaps in a `## Fleet Profile: SOLO OWNER-OPERATOR` section
- API/UI: `TenantController.UpdateTenantPresets`, `tms-portal/core/services/tenant.service.ts` (`isSoloMode`), `core/services/sidebar-nav.service.ts` (`SOLO_HIDDEN_ITEMS`), `admin-portal/shared/components/tenant-form/`, `admin-portal/pages/tenants/tenant-edit/`
- Note: solo relies on Owner holding `Permission.Driver.*` (`Shared.Identity/Policies/TenantRolePermissions.cs`), and driver-facing queries keying off `Truck.MainDriverId`/`SecondaryDriverId` rather than the role. There is no multi-role model
- Seed: `DbMigrator` tenants set `Presets` on `Models/DemoTenantConfig.cs`; `solo` also uses `SeedData/solo.json`, `SeedDataKey` and `DataScale`

## AI dispatch

### Dispatch conversations & turns

Tenant-shared, not per-user: any user with `Permission.Dispatch.View` can list/read a conversation;
`Permission.Dispatch.Manage` sends/cancels/renames/deletes. Handlers do not check ownership -
`CreatedById` is audit-only. Shares the conversation/turn machinery with the
[AI copilot](../docs/ai-copilot.md) (`AgentConversation`/`AgentMessage`, `AgentTurnService`,
`AgentTranscriptReplay`), scoped by `AgentConversationKind.Dispatch`; each turn is an `AgentSession`
with `Type == Dispatch`.

- Domain: `Entities/AIDispatch/AgentSession.cs`, `AgentConversation.cs`, `AgentMessage.cs`
- Application: `Modules/Integrations/AIDispatch/Commands/` (`Create`/`Rename`/`Delete`AIDispatchConversation, `SendAIDispatchMessage` - concurrency guard + quota gate, mirrors `SendAICopilotMessage`; `CancelAIDispatchTurn`), `Modules/Integrations/AIDispatch/Queries/` (`GetAIDispatchConversations` - no owner filter, `GetAIDispatchConversationById` - messages + decisions + per-turn `AgentSessionDto` stats)
- Infrastructure: `Infrastructure.AI/Agents/Dispatch/AIDispatchService.cs` (thin adapter onto the shared `AgentTurnService`) + `DispatchAgentSurface.cs` (`IAgentSurface`: no per-caller tool scoping, tenant-wide broadcasts) + `AIDispatchConversationBuilder.cs` (system prompt, transcript replay via `AgentTranscriptReplay.LoadAsync`); planning runs on `Tools/Dispatch/GetDispatchCandidatesTool.cs` (type rule, HOS, deadhead, `EquipmentFit`, eligibility in one call); `Infrastructure.Communications/SignalR/Hubs/AIDispatchHub.cs` (tenant-wide `dispatch-board:{tenantId}` group, mounted at `/hubs/ai-dispatch`)
- Jobs: `Logistics.API/Jobs/AIDispatchTurnJob.cs` (re-checks `AgenticDispatch` inside the job, mirrors `AICopilotTurnJob`)
- API/UI: `AIDispatchController.cs` (`ai/dispatch/conversations` CRUD + messages + cancel, mirroring `AICopilotController`'s route style), `tms-portal/pages/ai-dispatch/`

### Dispatch decisions

- Domain: `Entities/AIDispatch/AgentDecision.cs`
- Application: `Modules/Integrations/AIDispatch/Commands/Approve*`, `Reject*` - append a System note to the decision's conversation transcript and broadcast it tenant-wide, but only when the session has a `ConversationId` (old sessions may predate conversations)
- Infrastructure: `Infrastructure.AI/Agents/AgentDecisionProcessor.cs`
- API/UI: (under `ai-dispatch/`)

### Dispatch policy (learned preferences)

- Domain: `Entities/AIDispatch/AIDispatchPolicy.cs` (one row per tenant; `GeneratedContent` is job-owned, `ManualContent` is dispatcher-owned)
- Application: `Modules/Integrations/AIDispatch/Services/AIDispatchPolicyLearner.cs` + `DecisionHistoryDigest.cs` + `AIDispatchPolicyPrompt.cs`; `Commands/{Update,Regenerate,Delete}AIDispatchPolicy/`, `Queries/GetAIDispatchPolicy/`
- Infrastructure: injected into the prompt by `Infrastructure.AI/Agents/Dispatch/AIDispatchSystemPrompt.cs` (`LearnedDispatchPolicy`), loaded in `AIDispatchConversationBuilder`
- Jobs: `Logistics.API/Jobs/AIDispatchPolicyLearningJob.cs` (nightly, `Cron.Daily(4)`)
- API/UI: `AIDispatchController` `ai/dispatch/policy`, `tms-portal/pages/ai-dispatch/dispatch-policy/`

### Tool registry

- Infrastructure: `Infrastructure.AI/Tools/` (catalogue, schema/binding) + `Tools/{Dispatch,Financial,Operations,Compliance,LoadBoard,Intermodal,Negotiation}/`
- API/UI: shared with `Logistics.McpServer` and the AI copilot
- Behavior metadata (`IsWrite`, `RequiredPermission`, `DecisionType`, `RequiredFeature`) lives on
  each `AgentToolDefinition` - there is no separate write-tool list anywhere
- Intermodal reads: `Tools/GetContainerStatusTool.cs` (ISO 6346), `Tools/GetTerminalInfoTool.cs` (UN/LOCODE).
  Gated by `TenantFeature.IntermodalContainers` - the schemas, the prompt section and the tools' own
  guard all move together; MCP lists every tool, so the guard inside each tool is the real gate.

### Broker rate negotiation (email)

See [docs/broker-email-negotiation.md](../docs/broker-email-negotiation.md). Gated by
`TenantFeature.AIRateNegotiation` + `Permission.Negotiation.View/Manage`. Every outbound offer is an
approved `AgentDecision`; inbound mail can only append a message and ask for a turn.

- Domain: `Entities/Negotiation/RateNegotiation.cs` (owns the reply token and the round cap), `NegotiationMessage.cs`, `LaneRateFloor.cs`; `Entities/InboundEmailRoute.cs` (**master** DB - maps a reply token to a tenant); `AgentDecision.NegotiationId`, `TenantSettings.DefaultRateFloorPerMile`
- Application: `Modules/Integrations/Negotiation/` - `Services/` (`LaneRateFloorResolver`, `NegotiationEmailComposer` - the one owner of broker-facing formatting and sanitization, `NegotiationTurnStarter`, `EmailReplyParser`), `Commands/` (`ProposeCounterOffer` - floor gate then credit gate then send-before-persist; `CloseNegotiation`; `ProcessInboundNegotiationEmail`; lane-floor CRUD), `Queries/` (`GetNegotiations`, `GetNegotiationById`, `PreviewCounterOffer`, `GetRateFloorContext`, `GetLaneRateFloors`); `Modules/Integrations/Webhooks/Commands/ProcessResendWebhook/`; shared credit gate in `Modules/Integrations/LoadBoard/Services/BrokerCreditGate.cs`
- Infrastructure: `Infrastructure.Communications/Email/` (`ResendThreadedEmailSender` - reply-to plus `In-Reply-To`/`References`, `ResendInboundEmailReader` - the Received Emails API, `ResendWebhookVerifier`, `Templates/BrokerCounterOffer.liquid`); `Infrastructure.Integrations.Common/WebhookSignature.VerifySvix`; `Infrastructure.AI/Tools/Negotiation/` (`get_rate_floor`, `get_negotiation_thread`, `propose_counter_offer`)
- Jobs: `Logistics.API/Jobs/NegotiationExpirySweepJob.cs` (every 6 hours - expires lapsed threads and revokes their routes), `NegotiationWakeJob.cs` (delayed retry when a reply lands mid-turn)
- API/UI: `NegotiationController.cs` (`negotiations` - list, detail, close, decision preview; sending is deliberately absent), `RateFloorController.cs` (`ratefloors`), `POST /webhooks/resend`, `tms-portal/pages/ai-dispatch/negotiations/` + `negotiation-details/` + `rate-floors/`
- Demo data: `DbMigrator/Seeders/FakeData/NegotiationSeeder.cs` - lane floors, `Demo` load board listings and one thread per status per demo tenant; the tenant-wide fallback floor comes from `DemoTenantsSeeder`

## AI copilot

See [docs/ai-copilot.md](../docs/ai-copilot.md). Conversational agent sharing the dispatch tool
registry, `AgentLoopRunner`, decisions, and quota. Gated by `TenantFeature.AICopilot` +
`Permission.Copilot.View/Manage`; tool catalogue scoped per user.

### Conversations & turns

- Domain: `Entities/AIDispatch/AgentConversation.cs`, `AgentMessage.cs` (transcript with tool_use ids; `Kind` distinguishes the per-user Copilot conversations here from the tenant-shared Dispatch ones above - every copilot query/list/get filters `Kind == Copilot`); `AgentSession.Type == Copilot` per turn
- Application: `Modules/Integrations/AICopilot/Commands/` (send returns the server-stamped `UserMessageCreatedAt` so the client's optimistic echo never sorts by a browser clock; rename via `RenameAICopilotConversation`), `Queries/` (incl. `GetAICopilotQuotaStatus` - the shared AI quota behind `Permission.Copilot.View` for tenants without AgenticDispatch)
- Infrastructure: `Infrastructure.AI/Agents/` (`AgentTurnService.cs` - the shared turn lifecycle every conversational agent runs through, parameterized by `IAgentSurface`; `AgentTranscriptCodec.cs`, `AgentTranscriptReplay.cs`) and `Agents/Copilot/` (`AICopilotService.cs` - thin adapter onto `AgentTurnService` + `CopilotAgentSurface.cs`, `AICopilotConversationBuilder.cs`, `AICopilotSystemPrompt.cs` - dispatch-action guardrails render only when the caller's tool catalogue includes the dispatch write tools); `Infrastructure.Communications/SignalR/Hubs/CopilotHub.cs` (`/hubs/copilot`, authorized, per-user groups)
- Jobs: `Logistics.API/Jobs/AICopilotTurnJob.cs`
- API/UI: `AICopilotController.cs` (`ai/copilot` - conversations CRUD + rename, messages, cancel, `quota`, decisions approve/reject), `tms-portal/shared/layout/copilot-drawer/`; state in `core/store/copilot.store.ts` (persists drawer width, polls stuck Running turns, gates the launcher on `Copilot.View`)

### Copilot tools (loads, customers, invoicing, expenses, maintenance, detention, inspections)

- Infrastructure: `Tools/SearchLoadsTool.cs`, `GetLoadTool.cs`, `Operations/GetDetentionTool.cs`, `Compliance/GetConditionReportsTool.cs`, `SearchCustomersTool.cs`, `GetInvoicesTool.cs`, `GetInvoiceTool.cs`, `SearchExpensesTool.cs`, `GetExpenseStatsTool.cs`, `GetUpcomingMaintenanceTool.cs`, `CreateLoadInvoiceTool.cs`, `SendInvoiceTool.cs`, `CreatePaymentLinkTool.cs` (+ shared `AgentToolJson.cs` schema and binding)

### LLM providers

- Infrastructure: `Infrastructure.AI/Llm/` (`LlmProviderFactory`) and `Llm/Providers/` (`AnthropicLlmProvider`, `OpenAIResponsesLlmProvider`, `OpenAICompatibleLlmProvider` - DeepSeek etc.)

### Quota / pricing

- Domain: `Entities/Subscription/SubscriptionPlan.cs` (`WeeklyAIBudgetUsd`, required; unsubscribed tenants fall back to the Enterprise budget via `Application.Abstractions/AIDispatch/AIWeeklyBudget.cs`)
- Application: `Application.Abstractions/AIDispatch/IAIQuotaService.cs` (port)
- Infrastructure: `Infrastructure.Persistence/Services/AIDispatch/AIQuotaService.cs` (cost-based quota tracking), `Infrastructure.AI/Llm/LlmPricing.cs` (per-token pricing), `Application.Abstractions/Payments/Stripe/AIOverageBilling.cs` (cost → Stripe metered units; shared by Stripe metering and the tenant-visible accrued-charges figure)
- API/UI: (quota bar in `tms-portal/pages/ai-dispatch/`), `admin-portal/pages/ai-settings/tenant-quotas/` (per-tenant usage + revenue-vs-LLM-cost margin report)
- Quota numbers live in `SubscriptionPlanSeeder` + admin portal only - never exposed to tenants (tenant API returns a percentage)

### Global AI settings

- Application: `Modules/Platform/AISettings/` (`GetAISettings`, `UpdateAISettings`); `Abstractions/AIDispatch/LlmModelCatalog.cs`
- Infrastructure: `SystemSettingsService` (`AI.Model`/`AI.ReasoningEffort` keys)
- API/UI: `AISettingsController.cs`, `admin-portal/pages/ai-settings/`

### Commercial license

- Domain: `ProductLicenseHeartbeat` (master), `Options/ProductLicenseOptions.cs` (`License__Key`, `License__HeartbeatUrl`, `License__HeartbeatEnabled`)
- Application: `Modules/Platform/ProductLicense/` - `Services/ProductLicenseKeyValidator` (ES256 JWT against the embedded public key), `ProductLicenseService` (env var wins over `SystemSettings`, 5 min cache), `ProductLicenseHeartbeatService` (builds the report, sends it through `IProductLicenseHeartbeatSender`); commands `SetProductLicenseKey`, `RecordProductLicenseHeartbeat`; queries `GetProductLicenseStatus`, `GetProductLicenseDiscovery`
- Infrastructure: `Logistics.Infrastructure.Licensing` - `ProductLicenseHeartbeatSender` posts the daily report
- API/UI: `ProductLicenseController.cs` (`/license`, `/license/key`, anonymous `/license/heartbeat` and `/.well-known/logisticsx.json`), `Jobs/ProductLicenseHeartbeatJob.cs`, `HostDefaults/ProductVersionHeaderExtensions.cs` (`X-LogisticsX-Version`), `shared/services/product-license.service.ts` + `ui-license-banner` (all three portals), `admin-portal/pages/license/`
- Keys are issued with `tools/Logistics.LicenseIssuer` (`keygen`, `issue`, `inspect`); legal text in `COMMERCIAL-LICENSE.md`, `CLA.md`

### Background runner

- API/UI: `Jobs/AIDispatchTurnJob.cs`

### MCP server

- API/UI: `Logistics.McpServer/` (`McpToolSurface` over `IAgentToolRegistry`)

## Compliance & safety

### ELD / HOS logs

- Domain: `Entities/Eld/HosLog.cs`, `HosViolation.cs` (`RuleSetCode`), `DriverHosStatus.cs`, `ValueObjects/HosLimits.cs`
- Application: `Modules/Compliance/Eld/Commands/`, `Modules/Compliance/Eld/Queries/GetHosLimits/`, `Modules/Compliance/Eld/Services/RuleSetSelector.cs` (region → FMCSA / EU 561/2006)
- Infrastructure: `Infrastructure.Integrations.Eld/` (Samsara, Motive, TT ELD, Geotab, Demo). Casing comes from `IntegrationJsonOptions` (camel/snake), reads from `TryGetFromJsonAsync` - all in `Integrations.Common` (429/5xx retries come from the host-wide standard resilience handler, GET only - unsafe methods are never retried). Samsara follows `pagination.endCursor`, picks its host from `EldProviderConfiguration.Region` (us/eu/ca) and implements `IEldGpsTrackingProvider` (`/fleet/vehicles/stats`)
- API/UI: `EldController.cs` (incl. `/eld/rules/limits`), `tms-portal/pages/eld/` (`EldRulesService`, region-aware dashboard, `eld-hos-logs/`)
- Docs: [docs/eld-samsara-setup.md](../docs/eld-samsara-setup.md) (tenant setup: token scopes, webhook URL + secret)

### ELD provider config

- Domain: `Entities/Eld/EldProviderConfiguration.cs` (`ExternalAccountId` = TT ELD USDOT, `Region` = Samsara data region), `EldDriverMapping.cs`, `EldVehicleMapping.cs`
- Application: `Commands/CreateEldProviderConfiguration/` (initializes the provider from the unsaved config before validating credentials), `MapEldDriver` / `MapEldVehicle` / `ApplyEldMappingSuggestions`, `Queries/GetEldMappingSuggestions/`, `Services/EldMappingSuggestionService.cs` + `EldMappingMatcher.cs` (driver by email → license → name, vehicle by VIN → truck number; unique keys only)
- Infrastructure: `Infrastructure.Integrations.Eld/` factory
- API/UI: `EldController.cs` (`/eld/providers/{id}/{drivers,vehicles,mappings,vehicles/mappings,suggestions}`), `tms-portal/pages/eld/eld-providers/` (connect dialog, webhook URL to copy), `eld-mappings/` (suggestions + driver + vehicle mapping cards)

### ELD sync

- Application: `Modules/Compliance/Eld/Services/EldSyncService.cs` - the only writer of ELD data: `SyncLiveAsync` (HOS clocks + vehicle GPS through `ITruckLocationRecorder`, broadcast via `IRealtimeLiveTrackingService`), `SyncHistoryAsync` (8-day rolling window of `HosLog` rows replaced from the provider, violations upserted). `Commands/ProcessEldWebhook/` (resolves the tenant from the URL, then signature, idempotency, dispatch) and `SyncAllDriversHosStatus` both call it
- Infrastructure: `WebhookSignature.VerifySamsara` (`v1:{timestamp}:{body}`, base64 secret, 5-min window); `VerifyHmacSha256` for Motive / Geotab
- API/UI: `Jobs/EldSyncJob.cs` (`eld-sync` every 5 min, `eld-history-sync` every 30 min, gated on `TenantFeature.Eld`), webhooks: `/webhooks/eld/{samsara,motive,geotab}/{tenantId}`

### DVIR inspections

- Domain: `Entities/Safety/DvirReport.cs`, `DvirDefect.cs`
- Application: `Modules/Compliance/Dvir/Commands/`, `Modules/Compliance/Dvir/Queries/`
- API/UI: `DvirController.cs` (incl. `GET /dvir/trucks/{truckId}/open-defects`), `tms-portal/pages/safety/`
- Feature flag: `TenantFeature.Dvir` (Starter+; enabled by default; gates commands and queries - separate from `Safety`, which stays Professional)

### Vehicle inspections

- Domain: `Entities/Inspection/LoadConditionReport.cs` (`LoadVehicleId` = the load vehicle inspected), `ConditionDefect.cs` (`Zone` / `DamageType` / `PhotoDocumentId` for diagram defects); enums `VehicleDamageZone`, `VehicleDamageType` + `VehicleDamageExtensions` (BOL codes, zone → part category) in `Domain.Primitives/Enums/Inspection/`
- Application: `Modules/Compliance/Inspections/Commands/`, `Modules/Compliance/Inspections/Queries/`
- API/UI: `InspectionController.cs` (list/get/create + `GET /inspections/parts`), `VinController.cs` (`GET /vins/{vin}`)
- Feature flag: `TenantFeature.Safety` **or** `TenantFeature.VehicleTransport` (`[RequiresFeature]` takes several features, any one admits); car haulers get condition reports and VIN decode on any plan

### Accidents

- Domain: `Entities/Safety/AccidentReport.cs`
- Application: `Modules/Compliance/Accidents/Commands/`, `Modules/Compliance/Accidents/Queries/`
- API/UI: `AccidentController.cs`, `tms-portal/pages/safety/`

### Driver behavior

- Domain: `Entities/Safety/DriverBehaviorEvent.cs`
- Application: `Modules/Compliance/Safety/Commands/`, `Modules/Compliance/Safety/Services/`
- API/UI: `DriverBehaviorController.cs`

### Maintenance

- Domain: `Entities/Maintenance/`
- Application: `Modules/Operations/Maintenance/Commands/`, `Modules/Operations/Maintenance/Queries/`
- API/UI: `MaintenanceController.cs`, `tms-portal/pages/maintenance/`, `Jobs/MaintenanceReminderJob.cs`

### Driver licensing

- Domain: `Entities/Employee/DriverLicense.cs`
- Application: `Modules/IdentityAccess/Employees/Commands/{Create,Update,Delete}DriverLicense/`, `Application.Abstractions/Dispatch/IDispatchEligibilityService`, `Modules/Compliance/Safety/Services/LicenseExpiryReminderService`
- Infrastructure: `Infrastructure.AI/Tools/Dispatch/CheckDispatchEligibilityTool.cs`
- API/UI: `EmployeeController` (extended), `tms-portal/pages/employees/components/driver-licenses-tab/`, mobile `feature/more/LicensesScreen.kt`, `Jobs/LicenseExpiryReminderJob.cs`

### ADR / Hazmat

- Domain: `Truck.AdrEquipment`, `Load.HazmatClass`, `ValueObjects/AdrEquipment.cs`
- Application: `IDispatchEligibilityService` (gates `DispatchLoad`, `BulkDispatchLoads`, `DispatchTrip` handlers)
- API/UI: (under Loads / Trucks pages - hazmat fields on load-form, ADR fieldset on truck-form)

### IFTA reporting

- Domain: `Entities/Ifta/` (`TruckLocationHistory`, `TruckJurisdictionMileage`, `IftaQuarterSnapshot`), `Entities/IftaTaxRate.cs` (master)
- Application: `Modules/Compliance/Ifta/` (`TruckLocationRecorder`, `IftaReportService`, quarter-close + filing-reminder services, `IftaQuarters`), `Modules/Compliance/Ifta/TaxRates/` (admin rate CRUD), `Modules/Platform/Reports/Queries/Ifta/`
- Infrastructure: `Infrastructure.Routing/Geospatial/JurisdictionResolver.cs` (NTS + embedded boundaries, built once at startup by `JurisdictionResolverWarmup`); `Infrastructure.Documents/Pdf/Ifta/`
- **Traps**: closed quarters are served verbatim from `IftaQuarterSnapshot.ReportJson` (sole source of truth, deliberately not denormalized into columns). Tax-rate `(Year, Quarter, Jurisdiction)` and mileage `(Truck, Date, Jurisdiction)` uniqueness are raw-SQL unique indexes (NULLS NOT DISTINCT on region) created in migrations, not EF config - complex-type members can't be in an EF index, so a model rebuild won't recreate them. Write paths still probe `IftaTaxRateUniqueness.FindConflictAsync` for a readable error; `TruckLocationRecorder` upserts mileage with `ON CONFLICT`, which needs the index
- API/UI: `ReportController` `/reports/ifta` + `/reports/ifta/pdf`, `IftaTaxRateController.cs` `/ifta/rates`, `Jobs/IftaQuarterCloseJob.cs`, `Jobs/IftaFilingReminderJob.cs`, `tms-portal/pages/reports/ifta-report/`, `admin-portal/pages/ifta-rates/`

### Privacy (GDPR)

- Domain: `Entities/Privacy/`
- Application: `Modules/Compliance/Privacy/` (Commands, Queries, Services)
- Infrastructure: `Infrastructure.Documents/Privacy/`, `Infrastructure.Persistence/Services/Privacy/DataAnonymizer.cs`
- API/UI: `PrivacyController.cs`, `Jobs/Data{ExportProcessing,Deletion,Retention,ExportExpiry}Job.cs`, `<ui-cookie-banner>`, `admin-portal/pages/data-requests/`

## Financial

### Invoices (TPH)

- Domain: `Entities/Invoice/Invoice.cs` + `LoadInvoice`, `PayrollInvoice`, `Invoice.RecalculateTotals`, `InvoiceTaxLine`; `Domain.Primitives/Enums/Invoice/InvoiceStatuses.cs` (`Open`, `Receivable`) - every receivables figure filters on these; never list statuses inline
- Application: `Modules/Financial/Invoices/Commands/`, `Modules/Financial/Invoices/Queries/`, `IInvoiceTaxApplier`
- API/UI: `InvoicesController.cs` + `preview-tax`, `tms-portal/pages/invoices/`, `customer-portal/pages/invoices/`

### Accessorials and detention terms

- Domain: `Entities/Load/AccessorialCharge.cs` (tenant catalog), `TenantSettings.DetentionFreeMinutes`/`DetentionRatePerHour`, `Customer.DetentionFreeMinutes`/`DetentionRatePerHour`
- Application: `Modules/Financial/Accessorials/` (catalog CRUD, customer detention terms; tenant terms save through `UpdateTenant`)
- API/UI: `AccessorialController.cs` (`/accessorials`), `CustomerController` (`/customers/{id}/detention`), `tms-portal/pages/settings/accessorial-settings/`, tenant terms in `pages/settings/company-settings/`, `pages/customers/components/customer-detention-terms/`

### Factoring

- Domain: `Entities/Factoring/` (`FactoringCompany` + `EstimateAmounts`, `FactoringBatch` = schedule of accounts with `IneligibilityReason`/`Submit`/`RefreshStatus`, `FactoringBatchItem` with `Fund`/`Reject`/`ReleaseReserve`/`ChargeBack`, `FactoringAmounts`), `Customer.FactoringCompanyId`, `LoadInvoice.Factoring.cs` (`FactoringStatus`)
- Application: `Modules/Financial/Factoring/` (company CRUD, `SubmitInvoicesToFactor` - POD/BOL check, attachments, email, `RecordFactoringFunding`, `ReleaseFactoringReserve`, `ChargeBackFactoredInvoice`, batch queries, `Services/FactoringBatchDtos`); QBO in `Accounting/Services/AccountingReceivablesSyncService` (sale payment to the clearing account) and `AccountingPayablesSyncService` (fee as a purchase)
- Infrastructure: `Infrastructure.Documents/Pdf/Factoring/FactoringSchedulePdfService.cs`, `Pdf/Invoice/InvoiceRemitToSection.cs` (notice of assignment); attachments via `ThreadedEmail.Attachments` in `Communications/Email/ResendThreadedEmailSender.cs`; template `FactoringSchedule.liquid`
- **Trap**: funding records a `Payment` for the invoice total (the invoice reads Paid), so anything customer-facing must check `Customer.FactoringCompany` before showing paid/balance - the PDF, the invoice email, the portal list and the public pay page all do
- API/UI: `FactoringController.cs` (`/factoring/companies`, `/factoring/batches`), `tms-portal/pages/settings/factoring-settings/`, `tms-portal/pages/invoices/factoring-batches-list/` + `factoring-batch-details/` + `components/submit-to-factor-dialog/`. Docs: `docs/invoices.md#factoring`

### Tax engine

- Domain: `Entities/TenantTaxRate.cs`, `Invoice.TaxBehavior`, `Invoice.TaxBreakdownJson`, `ValueObjects/TaxJurisdiction.cs` - build jurisdictions with `TaxJurisdiction.Create(country, region)` on every write path; it upper-cases and normalizes a blank region to null, which is what the equality comparisons rely on
- Application: `Application.Abstractions/Tax/ITaxCalculator`, `Modules/Financial/Tax/Services/IInvoiceTaxApplier`, `Modules/Financial/Tax/Commands/`, `Modules/Financial/Tax/Queries/`, `Modules/Financial/Invoices/Queries/PreviewInvoiceTax/`
- Infrastructure: `Infrastructure.Tax/` (`StripeTaxCalculator`, `ManualTaxCalculator`, `StripeTaxConfigService`, `EUVatRates`/`USSalesTaxRates`/`OtherCountryRates`); `Infrastructure.Documents/Pdf/Invoice/` VAT block
- API/UI: `TaxController.cs`, `TaxRatesController.cs`, `tms-portal/pages/settings/components/tenant-tax-rates-card/`

### Payments

- Domain: `Entities/Payment/Payment.cs` (`StripePaymentIntentId`)
- Application: `Modules/Financial/Payments/Commands/`, `Modules/Financial/Payments/Queries/`
- Infrastructure: `Infrastructure.Payments/Stripe/StripePaymentService.cs`
- API/UI: `PaymentController.cs`, `PublicPaymentController.cs`

### Payment links

- Domain: `Entities/Payment/PaymentLink.cs`
- Application: `Modules/Financial/PaymentLinks/Commands/`, `Modules/Financial/Payments/Commands/CreatePublicCheckoutSession/`, `Modules/Financial/PaymentLinks/Queries/`
- API/UI: `customer-portal/pages/payment/` (redirects to Stripe-hosted Checkout)

### Stripe subscriptions

- Domain: `Entities/Subscription/Subscription.cs`
- Application: `Modules/IdentityAccess/Subscriptions/Commands/`, `Modules/IdentityAccess/Subscriptions/Queries/`
- Infrastructure: `Infrastructure.Payments/Stripe/StripeSubscriptionService.cs`
- API/UI: `SubscriptionController.cs`, `admin-portal/pages/subscriptions/`, `tms-portal/pages/subscription/`

### Subscription plans

- Domain: `Entities/Subscription/SubscriptionPlan.cs`, `PlanFeature.cs`
- Application: `Modules/IdentityAccess/Subscriptions/Queries/`
- Infrastructure: `Infrastructure.Payments/Stripe/StripePlanService.cs`
- API/UI: `admin-portal/pages/plans/`

### Stripe Connect

- Domain: `Entities/Tenant.cs` (`StripeConnectedAccountId`), `Entities/Employee.cs` (`Address`, `StripeConnectedAccountId`)
- Application: `Modules/Financial/StripeConnect/Commands/`, `Modules/Financial/StripeConnect/Queries/`, `Modules/Financial/Payroll/Commands/SetupEmployeePayout/`
- Infrastructure: `Infrastructure.Payments/Stripe/StripeConnectService.cs`, `StripeCapabilities.cs` (country-aware capability map: SEPA/iDEAL/Bacs/ACH)
- API/UI: `StripeConnectController.cs`, `tms-portal/pages/settings/billing-settings/`

### Stripe webhooks

- Application: `Modules/Integrations/Webhooks/Commands/ProcessStripEvent/` (`customer.subscription.*`, `payment_intent.processing|succeeded|payment_failed`, `account.updated`, `checkout.session.completed`)
- Infrastructure: `Infrastructure.Payments/Stripe/`
- API/UI: `WebhookController.cs` (`/webhooks/stripe`)

### Payroll

- Domain: `Entities/Invoice/PayrollInvoice.cs`
- Application: `Modules/Financial/Payroll/` (Commands, Queries, Services)
- API/UI: `tms-portal/pages/payroll/`, `Jobs/PayrollGenerationJob.cs`
- Settlements: `Entities/Invoice/PayrollInvoice.Settlement.cs` (`ReplaceSettlementLines`, signed deduction lines; `SettlementInputs`), `Entities/Employee/RecurringDeduction.cs`, `CashAdvance.cs`, `Employee.PayExcludesFuelSurcharge`/`PayExcludesAccessorials`/`DeductFuelCardCharges`, `FuelCardTransaction.PayrollInvoiceId`; `Services/PayrollService.cs` (gross by salary type incl. `PerLoad`/`PerStop`, escrow balance, claims/releases advances and fuel); commands `UpdateDriverPayProfile`, `IssueCashAdvance`, `DeleteCashAdvance`; queries `GetDriverPayProfile`, `GetCashAdvances`. API: `EmployeeController` (`/employees/{id}/payprofile`), `CashAdvanceController` (`/cashadvances`: list, issue, delete). PDF: `Infrastructure.Documents/Pdf/Payroll/PayrollEarningsSection.cs`. UI: `pages/employees/components/driver-pay-profile/`, `cash-advances-panel/`, `pages/payroll/components/line-items-table/` (generated lines locked)

### Expenses

- Domain: `Entities/Expense/` (CompanyExpense, TruckExpense, BodyShopExpense)
- Application: `Modules/Financial/Expenses/Commands/`, `Modules/Financial/Expenses/Queries/`
- API/UI: `ExpenseController.cs`, `tms-portal/pages/expenses/`

### Fuel cards

- Domain: `Entities/FuelCards/` (`FuelCardProviderConfiguration`, `FuelCard`, `FuelCardTransaction`); `TruckExpense.PurchaseJurisdiction`/`FuelCardProvider`/`ExternalTransactionId`
- Application: `Modules/Integrations/FuelCards/` (Commands, Queries, `Services/FuelCardSyncService`)
- Infrastructure: `Infrastructure.Integrations.FuelCards/` (WEX, EFS, Demo; factory). WEX/EFS share `Providers/BearerFuelCardService.cs`; config binds once from the `FuelCards` section into aggregate `FuelCardsOptions` (nullable `Wex`/`Efs`, read as `options.Value.Wex ?? new WexOptions()`) - same shape as `LoadBoardOptions`/`EldOptions`
- API/UI: `FuelCardsController.cs` (providers, sync, transaction review queue), `Jobs/FuelCardSyncJob.cs`, `tms-portal/pages/fuel-cards/`
- **Trap**: `FuelCard` has no endpoints by design - it's an internal card→truck mapping table written by `FuelCardSyncService` and `AssignFuelCardTransactionTruckHandler`. It looks orphaned; deleting it breaks both

### Accounting sync (QBO)

- Domain: `Entities/Accounting/` (`AccountingProviderConfiguration` - tokens, `NeedsReconnect`, default item/accounts, `SyncFromDate`, item/category mappings as the `Mappings` JSON complex property (column `mappings`, `ValueObjects/Accounting/AccountingMappings`); `QboEntityMapping` - one row per pushed record; failed rows retry on every run)
- Application: `Modules/Integrations/Accounting/` - `Services/AccountingSyncService` (the only push path, via `AccountingReceivablesSyncService` and `AccountingPayablesSyncService`: selects only rows edited since their last push, batches lookups, 500 per type per run), `AccountingMappingService` (reads and writes `QboEntityMapping`: what is not up to date, push outcomes, retiring), `AccountingPayloads` (payload + content hash + QBO `requestid`), `AccountingConnectionService` (token refresh, flags reconnect on `invalid_grant`); Commands: Connect/DisconnectQuickBooks, UpdateQuickBooksSettings, SyncQuickBooks → `RunAccountingSync` (enqueued, also the sync-issues Retry); Queries: GetQuickBooksConnection/AuthUrl/Lookups/SyncIssues. Ports in `Application.Abstractions/Accounting/` (`IAccountingProviderService`, `IAccountingProviderFactory`, `IOAuthStateProtector`, `AccountingReconnectRequiredException`)
- Infrastructure: `Infrastructure.Integrations.Accounting/` (QBO OAuth2 + REST push: Customer, Vendor, Invoice + void, Payment + void (deleted payments, e.g. a factoring charge-back), Purchase, Bill; stale-SyncToken retry, `requestid` on creates, `intuit_tid` in errors); token cols encrypted via `Persistence/Converters/EncryptedStringConverter`; `Persistence/Services/Accounting/OAuthStateProtector` (10-minute state)
- API/UI: `AccountingController.cs` (+ anonymous `/accounting/quickbooks/callback` and `/disconnected`), `Jobs/AccountingSyncJob.cs` (15 min fan-out), `tms-portal/pages/settings/integrations-settings/` + `pages/settings/components/quickbooks-card/`, `quickbooks-settings-form/`, `quickbooks-sync-issues/`. Setup guide: `docs/accounting-quickbooks-setup.md`
- **Trap**: `GetQuickBooksConnection` and `DisconnectQuickBooks` are deliberately not feature-gated - a downgraded tenant must still be able to revoke access

## Communication

### Real-time messaging

- Domain: `Entities/Messaging/Conversation.cs`, `Message.cs`
- Application: `Modules/Integrations/Messaging/Commands/`, `Modules/Integrations/Messaging/Queries/`
- Infrastructure: `Infrastructure.Communications/SignalR/Hubs/ChatHub.cs`
- API/UI: `MessageController.cs`, `tms-portal/pages/messages/`

### Live tracking

- Application: `Modules/Operations/Tracking/Commands/`, `Modules/Operations/Tracking/Queries/`
- Infrastructure: `Infrastructure.Communications/SignalR/Hubs/TrackingHub.cs`, `SignalR/Services/SignalRTripTrackingService.cs` (port `Application.Abstractions/Routing/ITripTrackingService.cs`), `Infrastructure.Routing/Geocoding/` (Mapbox)
- API/UI: `TrackingController.cs`, `customer-portal/pages/tracking/`

### Notifications

- Domain: `Entities/Notification.cs`
- Application: `Modules/Platform/Notifications/Commands/`, `Modules/Platform/Notifications/Queries/`, `Application.Abstractions/Notifications/INotificationService.cs` (port)
- Infrastructure: `Infrastructure.Communications/Notifications/` (Firebase push), `SignalR/Hubs/NotificationHub.cs`
- API/UI: `NotificationController.cs`, `tms-portal/pages/notifications/`

### Email

- Infrastructure: `Infrastructure.Communications/Email/` (Resend, Fluid templates)

### Public tracking links

- Domain: `Entities/TrackingLink.cs`
- Application: `Modules/Operations/Tracking/Commands/`

## Identity & access

### Users

- Domain: `Entities/User.cs`, `UserTenantAccess.cs`
- Application: `Modules/IdentityAccess/Users/` (Commands, Queries, Services)
- API/UI: `UserController.cs`, `admin-portal/pages/users/`, `tms-portal/pages/settings/`

### Roles / permissions

- Domain: `Entities/Role/` (AppRole, TenantRole, claims); `Shared.Identity` `AppRoles`/`AppRolePermissions` (SuperAdmin, Admin)
- Application: `Modules/IdentityAccess/Roles/Queries/`
- API/UI: `RoleController.cs`

### App admins

- Domain: `Entities/Invitation.cs` (`Type=AppUser`, `AppRole`, nullable `TenantId`)
- Application: `Modules/IdentityAccess/Admins/` (Commands: AddAdmin/RevokeAdmin/Resend+CancelAdminInvitation, Queries: GetAdmins/GetAdminInvitations)
- API/UI: `AdminController.cs` (`/admins`, SuperAdmin-only), `admin-portal/pages/admins/`

### Invitations

- Domain: `Entities/Invitation.cs`
- Application: `Modules/IdentityAccess/Invitations/Commands/` (tenant-scoped: Employee/CustomerUser)
- API/UI: `InvitationController.cs`; app-admin invites via `AdminController`

### Auth / OAuth2

- API/UI: `Logistics.IdentityServer/` (Open.IdentityServer), all portal `pages/login/`

### Tenants

- Domain: `Entities/Tenant.cs`
- Application: `Modules/IdentityAccess/Tenants/Commands/`, `Modules/IdentityAccess/Tenants/Queries/`
- Infrastructure: `Infrastructure.Persistence/Services/Tenant/CurrentTenantAccessor.cs` (`ICurrentTenantAccessor`, resolves the current tenant), `TenantDatabaseService.cs`
- API/UI: `TenantController.cs`, `admin-portal/pages/tenants/` (+ `admin-portal/shared/components/tenant-form/`)
- Demo tenants (`us`, `eu`, `solo`) come from the `Tenants[]` config array read by `DbMigrator/Seeders/Infrastructure/DemoTenantsSeeder.cs`; see [environment-variables.md](../docs/configuration/environment-variables.md#demo-tenants-tenants)

### Impersonation

- Domain: `Entities/ImpersonationToken.cs`, `ImpersonationAuditLog.cs`
- API/UI: `ImpersonationController.cs`

### API keys (MCP)

- Domain: `Entities/ApiKey.cs`
- Application: `Modules/IdentityAccess/ApiKeys/Commands/`, `Modules/IdentityAccess/ApiKeys/Queries/`
- Infrastructure: `Logistics.McpServer/Authentication/`
- API/UI: `ApiKeysController.cs`

### Feature flags

- Domain: `Entities/Feature/TenantFeatureConfig.cs`, `TenantPresetCatalog.cs`
- Application: `Modules/IdentityAccess/Features/Commands/`, `Modules/IdentityAccess/Features/Queries/`, `Application.Abstractions/Features/IFeatureService.cs` (port)
- Infrastructure: `Infrastructure.Persistence/Services/Feature/FeatureService.cs` (`ApplyPresetFeaturesAsync` writes per-tenant rows from presets; admin-locked rows are kept; a feature with no row resolves through the tenant's presets - there are no platform-wide defaults)
- API/UI: `FeaturesController.cs` (admin roles only), `TenantFeaturesController.cs` (read-only, current tenant), `admin-portal/pages/features/`. Tenants cannot toggle features; see _Tenant presets and solo mode_

## Documents & storage

### Documents (POD, BOL, …)

- Domain: `Entities/Document/` (Load, Truck, Employee, Delivery)
- Application: `Modules/Integrations/Documents/Commands/`, `Modules/Integrations/Documents/Queries/`; `Services/DocumentDownloadService.cs` (`IDocumentDownloadService` - the one blob-to-download path, shared by `DownloadDocument`, the portal and public tracking downloads)
- Infrastructure: `Infrastructure.Documents/`
- API/UI: `DocumentController.cs`, `customer-portal/pages/documents/`

### PDF generation

- Application: `Application.Abstractions/Documents/` (`IInvoicePdfService`, `IIftaReportPdfService`, … ports)
- Infrastructure: `Infrastructure.Documents/Pdf/` (QuestPDF: invoices, payroll, IFTA, car hauler eBOL - `VehicleBolPdfService.cs` + `VehicleBol/VehicleDiagram.cs`, the zone geometry the TMS and driver app copy)

### PDF import / extraction

- Application: `Modules/Operations/Loads/Commands/ImportLoadFromPdf/`, `Application.Abstractions/Documents/IPdfDataExtractor.cs`
- Infrastructure: `Infrastructure.Documents/PdfImport/LlmPdfDataExtractor.cs` (LLM via `ILlmClient`; PdfPig text + Anthropic vision fallback)
- API/UI: `LoadController.ImportFromPdf` (`POST /loads/import`)

### VIN decoder

- Application: `IVinDecoderService.cs`
- Infrastructure: `Infrastructure.Vin/` (composite: NHTSA `DecodeWMI` for prefix/country + NHTSA `DecodeVinValues` for full US detail)
- API/UI: `truck-form` Decode VIN button + source badge

### Blob storage

- Application: `IBlobStorageService.cs`
- Infrastructure: `Infrastructure.Storage/Providers/` (Azure, Cloudflare R2, file system)
- API/UI: selected via `BlobStorage:Type` config (`azure` / `r2` / default)

## Load board

### Load board search

- Domain: `Entities/LoadBoard/LoadBoardListing.cs`, `LoadBoardConfiguration.cs` (`UserEmail` = DAT One seat, `MarketRatesEnabled` = RateView), `Enums/LoadBoardEquipmentType.cs` (the one equipment list; DAT codes in `Providers/Dat/DatEquipment.cs`)
- Application: `Modules/Integrations/LoadBoard/Commands/` (search, create/update/test provider, create load from listing), `Queries/`; `Services/LoadBoardProviderGateway.cs` - the only way to call a board: token refresh, one 401 retry, vendor error to per-provider `Result`
- Abstractions: `Application.Abstractions/LoadBoard/ILoadBoardProviderService.cs` (throws `LoadBoardApiException`; no booking call - no board has a booking API)
- Infrastructure: `Infrastructure.Integrations.LoadBoard/` - `Providers/Dat/` (two-step org + user token, v1 search, truck posts, RateView), `Demo` (Truckstop and 123Loadboard have no provider; the gateway reports them "not available yet"); `LoadBoardHttp.cs` throwing HTTP helper. API verification status in `docs/loadboard-dat-setup.md`
- API/UI: `LoadboardController.cs`, `tms-portal/pages/loadboard/` (`provider-dialog` add/edit, `loadboard.constants.ts` equipment + provider labels)

### Broker credit

- Domain: `Entities/LoadBoard/BrokerCreditRecord.cs`, `LoadBoardListing.BrokerCredit*`, `TenantSettings.MinBrokerCreditScore`
- Application: `Modules/Integrations/LoadBoard/Queries/GetBrokerCredit/`, booking gate in `Commands/BookLoadBoardLoad/`
- Infrastructure: `Infrastructure.Integrations.LoadBoard/Credit/` (`BrokerCreditService`, `FmcsaClient`); `Infrastructure.AI/Tools/LoadBoard/CheckBrokerCreditTool.cs`
- API/UI: `GET /loadboard/brokers/{mc}/credit`, credit badges on `tms-portal/pages/loadboard/`, threshold on `settings/company-settings/`

### Posted trucks

- Domain: `Entities/LoadBoard/PostedTruck.cs`
- Application: `Modules/Integrations/LoadBoard/Commands/PostTruckToLoadBoard`, `RemovePostedTruck`
- API/UI: `/loadboard/trucks`, `tms-portal/pages/loadboard/posted-trucks/`

### Background sync

- API/UI: `Jobs/LoadBoardSyncJob.cs` (reconciles active posts with the board every 30 min; hourly expiry cleanup)

## Settings & integrations

### Tenant settings

- Domain: `Entities/Tenant.cs` → `TenantSettings` VO; `McNumber`/`VatNumber`/`EoriNumber`/`CompanyRegistrationNumber`/`TaxResidencyCountry`; `Presets` (see _Tenant presets and solo mode_)
- Application: `Modules/IdentityAccess/Tenants/Commands/UpdateTenant` (regulatory IDs validated server-side via `RegexPatterns.VatNumber`/`McNumber`/`EoriNumber`; `AIEnabled` and `BlockAIOverage` toggles per tenant)
- API/UI: `tms-portal/pages/settings/company-settings/`

### System settings

- Domain: `Entities/SystemSettings.cs`
- Application: `ISystemSettingsService.cs`

### Captcha

- Application: `Application.Abstractions/Captcha/ICaptchaService.cs` (port)
- Infrastructure: `Infrastructure.Communications/Captcha/` (Google reCAPTCHA)

### Geocoding

- Application: `Application.Abstractions/Geocoding/IGeocodingService.cs` (port)
- Infrastructure: `Infrastructure.Routing/Geocoding/` (Mapbox; biased to `Tenant.Settings.Region` country + language)

### Trip optimization

- Application: `ITripOptimizer.cs`
- Infrastructure: `Infrastructure.Routing/Optimization/` (heuristic, Mapbox Matrix, composite)

## Marketing site

### Blog

- Domain: `Entities/BlogPost.cs`
- Application: `Modules/Platform/BlogPosts/Commands/`, `Modules/Platform/BlogPosts/Queries/`
- API/UI: `BlogPostController.cs`, `admin-portal/pages/blog-posts/`, `website/`

### Contact form

- Domain: `Entities/ContactSubmission.cs`
- Application: `Modules/Platform/Contacts/Commands/`, `Modules/Platform/Contacts/Queries/`
- API/UI: `ContactController.cs`, `admin-portal/pages/contact-submissions/`

### Demo requests

- Domain: `Entities/DemoRequest.cs`
- Application: `Modules/Platform/DemoRequests/Commands/`, `Modules/Platform/DemoRequests/Queries/`
- API/UI: `DemoRequestController.cs`, `admin-portal/pages/demo-requests/`

### Telegram bot

- Domain: `Entities/TelegramChat.cs`, `TelegramLoginState.cs`
- API/UI: `Logistics.TelegramBot/`

## Hosting

### Observability & backups

- API/UI: `Logistics.HostDefaults/` (`SentryExtensions.cs`, `SentryEventScrubber.cs`, `TenantSentryEventProcessor.cs`), `Logistics.API/Jobs/HangfireHealthCheck.cs` (`hangfire` health check, registered in `Setup.cs`), Angular `projects/shared/src/lib/errors/error-tracking.ts` (lazy Sentry SDK)
- Deploy: `deploy/db-backup/pg-backup.sh` / `pg-restore.sh`, `deploy/docker-compose.backup.yml`
- Guide: [docs/deployment/observability-and-backups.md](../docs/deployment/observability-and-backups.md)

## Tests

All under `test/` (singular):

- `Logistics.Application.Tests/` - application handlers, services
- `Logistics.Architecture.Tests/` - layering/dependency boundaries; discovers projects, never hard-code a list
- `Logistics.Infrastructure.AI.Tests/` - AI agent, tools, prompts, pricing (quota lives in `Logistics.Infrastructure.Persistence.Tests/`, next to `AIQuotaService`)
- `Logistics.Infrastructure.{Communications,Documents,Payments,Persistence,Routing,Tax,Vin}.Tests/` - per-module
- `Logistics.HostDefaults.Tests/` - Sentry scrubbing and tenant tagging
- `Logistics.Mediator.Tests/` - the in-repo mediator

## Updating this map

Add a `### Feature` entry when a top-level feature lands. Don't add sub-features (e.g. _PaymentLink expiration_ belongs under Payment links - the entry already points to the right folders). If a path moves, update the line, don't leave a stale one. Only list the layers a feature actually touches.
