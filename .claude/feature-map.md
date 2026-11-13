# Feature map

Answers "where", only for what the conventions below don't make obvious. For "how it works" see [docs/](../docs/README.md): [architecture](../docs/architecture/overview.md), [module layout](../docs/architecture/module-layout.md), [domain model](../docs/architecture/domain-model.md), [AI dispatch](../docs/ai-dispatch.md), [invoices](../docs/invoices.md).

## Conventions

Path aliases used below: `Dom` = `src/Core/Logistics.Domain/Entities` · `App` = `src/Core/Logistics.Application/Modules` · `Abs` = `src/Core/Logistics.Application.Abstractions` · `Infra.X` = `src/Infrastructure/Logistics.Infrastructure.X` · `Ctl` = `src/Presentation/Logistics.API/Controllers` · `Jobs` = `src/Presentation/Logistics.API/Jobs` · `TMS` / `Cust` / `Admin` = `private/src/Client/Logistics.Angular/projects/{tms-portal,customer-portal,admin-portal}/src/app`.

- Entities: `Dom/{Module}/{Aggregate}/` (one-file aggregates sit in the module folder); enums: `src/Core/Logistics.Domain.Primitives/Enums/{Module}/{Aggregate}/`
- Commands, queries: `App/{Module}/{Feature}/{Commands,Queries}/{Verb}{Entity}/`; events, services, constants: `App/{Module}/{Feature}/{Events,Services,Constants}/`
- Domain events: `src/Core/Logistics.Domain/Events/`
- Ports implemented by infrastructure: `Abs/{Group}/`; ports implemented by Application: `Abs/ApplicationServices/`
- Implementations: `Infra.{Project}/`; provider integrations (ELD, fuel cards, load board, accounting, VIN, licensing) under `Infra.Integrations/{Provider type}/`, shared plumbing in `Infra.Integrations/Common/`
- EF configurations: `Infra.Persistence/Configurations/{Module}/{Aggregate}/`
- Controllers: `Ctl/{Module}/{Feature}Controller.cs`; webhooks: `WebhookController` + `App/Integrations/Webhooks/Commands/`
- SignalR hubs: `Infra.Communications/SignalR/Hubs/`
- Hangfire jobs: `Jobs/`, shared plumbing and the recurring schedule in `Jobs/Scheduling/`
- Error codes: `src/Shared/Logistics.Shared.Models/ErrorCodes.cs`, mirrored in `projects/shared/src/lib/errors/upgrade-handler.ts`
- Portal pages: `{TMS|Cust|Admin}/pages/{feature}/`; the fourth portal is `website`
- Driver app: `private/src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/`

## Exceptions

Rows give the non-obvious locations.

| Feature | Where |
| --- | --- |
| Load vehicles, eBOL (car hauler) | `Dom/Operations/Load/LoadVehicle.cs` · `App/Operations/Loads/Services/VehicleCapacityChecker.cs` · `App/Operations/Loads/Queries/GetVehicleBolPdf/` · `Infra.Documents/Pdf/VehicleBolPdfService.cs` · `Infra.Documents/Pdf/VehicleBol/VehicleDiagram.cs` · `TMS/pages/loads/components/load-vehicles-content/` · [guide](../docs/car-hauler-guide.md) |
| Load charges, LTL freight | `Dom/Operations/Load/Load.Billing.cs` · `Dom/Operations/Load/Load.Freight.cs` · `Dom/Financial/Invoice/LoadInvoice.Charges.cs` · `App/Operations/Loads/Services/LoadChargeService.cs` |
| Detention | `Dom/Operations/Trip/DetentionCalculator.cs` · `App/Operations/Loads/Queries/GetLoadDetention/` · `App/Operations/Trips/Services/DetentionAlertService.cs` · `Jobs/DetentionAlertJob.cs` |
| Accessorials, detention terms | `Dom/Operations/Load/AccessorialCharge.cs` · `App/Financial/Accessorials/` · `TMS/pages/settings/accessorial-settings/` · `TMS/pages/customers/components/customer-detention-terms/` |
| Customer updates (template emails) | `Dom/Operations/Load/Load.CustomerUpdates.cs` · `App/Operations/Loads/Services/CustomerUpdateService.cs` · `App/Operations/Loads/Events/LoadStatusChangedCustomerUpdateHandler.cs` · `App/Operations/Trips/Events/TripStopRunningLateCustomerUpdateHandler.cs` · `Infra.Communications/Email/Templates/` · `TMS/pages/loads/components/load-customer-updates-content/` |
| Trip ETA, stop arrive/depart | `Dom/Operations/Trip/Trip.Eta.cs` · `App/Operations/Trips/Services/EtaService.cs` · `Ctl/Operations/DriverController.cs` · `Infra.Routing/Optimization/` |
| Dispatch eligibility (HOS, license, ADR, truck/load type) | `src/Core/Logistics.Domain.Primitives/Enums/Operations/Truck/TruckLoadCompatibility.cs` · `Abs/ApplicationServices/IDispatchEligibilityService.cs` · `App/AI/AIDispatch/Services/DispatchEligibilityService.cs` |
| Trailer hooking | `App/Operations/Trailers/Services/TrailerHookService.cs` |
| Customer portal | `App/Operations/Portal/` · `Ctl/Operations/PortalController.cs` · `Cust/pages/shipments/` · `Cust/pages/documents/` · `Cust/pages/invoices/` · `Cust/pages/tracking/` |
| Drivers | `Ctl/Operations/DriverController.cs` · `Ctl/Identity/EmployeeController.cs` · `Dom/Identity/Employee/` · `TMS/pages/employees/` |
| Driver licenses | `Dom/Identity/Employee/DriverLicense.cs` · `App/Compliance/Safety/Services/LicenseExpiryReminderService.cs` · `Jobs/LicenseExpiryReminderJob.cs` · `TMS/pages/employees/components/driver-licenses-tab/` |
| Containers, drayage | `Dom/Operations/Container/Container.Drayage.cs` · `App/Operations/Containers/Services/` · `Jobs/ContainerDeadlineAlertJob.cs` · `TMS/shared/components/drayage/` |
| Time entries | `Dom/Operations/TimeEntry.cs` · `App/Operations/TimeEntries/` · `TMS/pages/timesheets/` |
| Dashboard, stats, reports | `App/Reporting/Stats/` · `App/Reporting/Reports/` · `Ctl/Reporting/` · `TMS/pages/home/` (`TMS/pages/dashboard/` is a redirect stub) · `TMS/pages/reports/` |
| Profitability | `App/Reporting/Reports/Services/ProfitabilityService.cs` · `App/Reporting/Reports/Services/ProfitEstimateService.cs` · `Abs/ApplicationServices/IProfitEstimateService.cs` · `Infra.AI/Tools/Financial/GetProfitabilityTool.cs` · `TMS/pages/reports/profitability-report/` |
| Onboarding checklist | `App/Tenancy/Onboarding/` · `Ctl/Tenancy/OnboardingController.cs` · `TMS/pages/home/components/onboarding-checklist/` |
| Data import | `App/Operations/DataImport/` · `Ctl/Operations/ImportController.cs` · `Infra.Documents/Spreadsheets/SpreadsheetReader.cs` · `TMS/pages/settings/data-import/` · [guide](../docs/data-import.md) |
| Tenant presets, solo mode | `src/Core/Logistics.Domain.Primitives/Enums/Tenancy/Tenant/TenantPreset.cs` · `Dom/Tenancy/Feature/TenantPresetCatalog.cs` · `App/Tenancy/Tenants/Commands/UpdateTenantPresets/` · `Infra.AI/Agents/Dispatch/AIDispatchSystemPrompt.cs` · `TMS/core/services/sidebar-nav.service.ts` |
| Feature flags | `Abs/Context/IFeatureService.cs` · `Infra.Persistence/Services/Feature/FeatureService.cs` · `Ctl/Tenancy/FeaturesController.cs` · `Ctl/Tenancy/TenantFeaturesController.cs` · `Admin/pages/features/` |
| Tenants, tenant resolution | `Infra.Persistence/Services/Tenant/CurrentTenantAccessor.cs` · `Infra.Persistence/Services/Tenant/TenantDatabaseService.cs` · `Admin/pages/tenants/` |
| AI dispatch conversations | `Dom/AI/Agents/AgentConversation.cs` · `App/AI/AIDispatch/` · `Infra.AI/Agents/Dispatch/` · `Infra.Communications/SignalR/Hubs/AIDispatchHub.cs` · `Jobs/AIDispatchTurnJob.cs` · `TMS/pages/ai-dispatch/` |
| AI copilot | `App/AI/AICopilot/` · `Infra.AI/Agents/Copilot/` · `Infra.Communications/SignalR/Hubs/CopilotHub.cs` · `Jobs/AICopilotTurnJob.cs` · `TMS/shared/layout/copilot-drawer/` · [doc](../docs/ai-copilot.md) |
| Agent turns, decisions (shared) | `Infra.AI/Agents/AgentTurnService.cs` · `Infra.AI/Agents/AgentDecisionProcessor.cs` · `Dom/AI/Agents/AgentDecision.cs` |
| Auto-approve, dispatch policy | `Dom/AI/AIDispatch/AIDispatchPolicy.cs` · `App/AI/AIDispatch/Services/AgentTrustService.cs` · `App/AI/AIDispatch/Services/AIDispatchPolicyLearner.cs` · `Jobs/AIDispatchPolicyLearningJob.cs` · `TMS/pages/ai-dispatch/dispatch-policy/` |
| AI trip alerts | `App/AI/AIDispatch/Services/TripAlertTurnStarter.cs` · `App/Operations/Trips/Services/TripAlertReporter.cs` · `Jobs/TripAlertWakeJob.cs` |
| Agent tools, MCP | `Infra.AI/Tools/{Dispatch,Financial,Operations,Compliance,LoadBoard,Intermodal,Negotiation}/` · `src/Presentation/Logistics.McpServer/` · [doc](../docs/mcp-server.md) |
| Broker negotiation | `Dom/AI/Negotiation/` · `App/AI/Negotiation/` · `Ctl/AI/NegotiationController.cs` · `Ctl/AI/RateFloorController.cs` · `App/Integrations/Webhooks/Commands/ProcessResendWebhook/` · `Jobs/NegotiationExpirySweepJob.cs` · `Infra.Communications/Email/` · `TMS/pages/ai-dispatch/negotiations/` · [doc](../docs/broker-email-negotiation.md) |
| AI quota, pricing | `Abs/AI/IAIQuotaService.cs` · `Abs/AI/LlmModelCatalog.cs` · `Infra.Persistence/Services/AIDispatch/AIQuotaService.cs` · `Admin/pages/ai-settings/` |
| Commercial license | `App/Tenancy/ProductLicense/` · `Infra.Integrations/Licensing/` · `src/Presentation/Logistics.HostDefaults/ProductVersionHeaderExtensions.cs` · `Admin/pages/license/` |
| ELD, HOS | `Dom/Compliance/Eld/` · `App/Compliance/Eld/Services/EldSyncService.cs` · `Infra.Integrations/Eld/` · `App/Integrations/Webhooks/Commands/ProcessEldWebhook/` · `Jobs/EldSyncJob.cs` · `TMS/pages/eld/` · [setup](../docs/eld-samsara-setup.md) |
| DVIR, accidents | `Dom/Compliance/Safety/` · `App/Compliance/Dvir/` · `App/Compliance/Safety/` · `TMS/pages/safety/` |
| Vehicle inspections, VIN | `Dom/Compliance/Inspection/` · `Ctl/Compliance/InspectionController.cs` · `Ctl/Compliance/VinController.cs` · `Infra.Integrations/Vin/` |
| IFTA | `Dom/Compliance/Ifta/` · `App/Compliance/Ifta/` · `App/Reporting/Reports/Queries/Ifta/` · `Infra.Routing/Geospatial/JurisdictionResolver.cs` · `Infra.Documents/Pdf/Ifta/` · `Jobs/IftaQuarterCloseJob.cs` · `TMS/pages/reports/ifta-report/` · `Admin/pages/ifta-rates/` |
| Privacy (GDPR) | `Dom/Compliance/Privacy/` · `Infra.Documents/Privacy/` · `Infra.Persistence/Services/Privacy/DataAnonymizer.cs` · `Admin/pages/data-requests/` |
| Invoices | `Dom/Financial/Invoice/` · `Ctl/Financial/InvoicesController.cs` · `TMS/pages/invoices/` · `Cust/pages/invoices/` |
| Payroll, settlements | `Dom/Financial/Payroll/` · `Ctl/Financial/InvoicesPayrollController.cs` · `Ctl/Financial/CashAdvanceController.cs` · `Jobs/PayrollGenerationJob.cs` · `Infra.Documents/Pdf/Payroll/` · `TMS/pages/payroll/` |
| Factoring | `Dom/Financial/Factoring/` · `App/Financial/Factoring/` · `Infra.Documents/Pdf/Factoring/` · `TMS/pages/settings/factoring-settings/` · `TMS/pages/invoices/factoring-batches-list/` |
| Tax | `Dom/Financial/TenantTaxRate.cs` · `App/Financial/Tax/` · `Infra.Payments/Tax/` · `Ctl/Financial/TaxController.cs` · `Ctl/Financial/TaxRatesController.cs` |
| Payments, payment links | `Dom/Financial/Payment/` · `Infra.Payments/Stripe/StripePaymentService.cs` · `Ctl/Financial/PublicPaymentController.cs` · `Cust/pages/payment/` · [doc](../docs/customer-payments.md) |
| Stripe subscriptions, plans, Connect | `Dom/Tenancy/Subscription/` · `Infra.Payments/Stripe/` · `Admin/pages/subscriptions/` · `Admin/pages/plans/` · `TMS/pages/subscription/` · `TMS/pages/settings/billing-settings/` |
| Stripe webhooks | `Ctl/Integrations/WebhookController.cs` · `App/Integrations/Webhooks/Commands/ProcessStripeEvent/` · `Infra.Payments/Stripe/StripeWebhookParser.cs` |
| QuickBooks sync | `Dom/Integrations/Accounting/` · `App/Integrations/Accounting/Services/AccountingSyncService.cs` · `Infra.Integrations/Accounting/` · `Jobs/AccountingSyncJob.cs` · `TMS/pages/settings/integrations-settings/` · [setup](../docs/accounting-quickbooks-setup.md) |
| Fuel cards | `Dom/Integrations/FuelCards/` · `App/Integrations/FuelCards/` · `Infra.Integrations/FuelCards/` · `Jobs/FuelCardSyncJob.cs` · `TMS/pages/fuel-cards/` |
| Load board, posted trucks | `Dom/Integrations/LoadBoard/` · `App/Integrations/LoadBoard/` · `Infra.Integrations/LoadBoard/` · `Ctl/Integrations/LoadboardController.cs` · `Jobs/LoadBoardSyncJob.cs` · `TMS/pages/loadboard/` · [setup](../docs/loadboard-dat-setup.md) |
| Broker credit | `App/Integrations/LoadBoard/Services/BrokerCreditChecker.cs` · `Infra.Integrations/LoadBoard/Credit/` · `Infra.AI/Tools/LoadBoard/CheckBrokerCreditTool.cs` |
| Live tracking, public tracking links | `Dom/Operations/Load/TrackingLink.cs` · `App/Operations/Tracking/` · `Infra.Communications/SignalR/Hubs/TrackingHub.cs` · `Infra.Communications/SignalR/Services/SignalRTripTrackingService.cs` · `Cust/pages/tracking/` |
| Notifications | `App/Operations/Notifications/Services/NotificationService.cs` · `Infra.Communications/Notifications/` · `Infra.Communications/SignalR/Hubs/NotificationHub.cs` |
| Telegram | `src/Presentation/Logistics.TelegramBot/` · `Dom/Operations/Notification/TelegramChat.cs` · `src/Presentation/Logistics.IdentityServer/Services/TelegramLoginService.cs` |
| App admins, invitations | `Dom/Identity/Invitation.cs` · `App/Identity/Admins/` · `App/Identity/Invitations/` · `Ctl/Identity/AdminController.cs` · `Admin/pages/admins/` |
| API keys (MCP) | `Dom/Identity/ApiKey.cs` · `Ctl/Identity/ApiKeysController.cs` · `src/Presentation/Logistics.McpServer/Authentication/` |
| PDF import | `App/Operations/Loads/Commands/ImportLoadFromPdf/` · `Infra.Documents/PdfImport/LlmPdfDataExtractor.cs` |
| Geocoding, routing, trip optimization | `Abs/Routing/` · `Infra.Routing/Geocoding/` · `Infra.Routing/Optimization/` |
| Observability, backups | `src/Presentation/Logistics.HostDefaults/` · `Jobs/Scheduling/HangfireHealthCheck.cs` · `deploy/db-backup/` · [doc](../docs/deployment/observability-and-backups.md) |

Add or update a row only when a feature breaks the convention.
