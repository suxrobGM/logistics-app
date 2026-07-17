# Feature Map

Stable starting points for finding code by feature. Read this before grepping. If a feature isn't here, it doesn't exist yet (or this file is stale - flag it).

Each feature lists only the layers it touches: **Domain** (entities/VOs), **Application** (commands/queries/services), **Infrastructure** (service implementations), **API/UI** (controllers, jobs, frontend pages). A missing label means that layer has nothing feature-specific.

## Where things go (conventions)

- **Domain entity**: `src/Core/Logistics.Domain/Entities/{Feature}/`
- **Commands**: `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Commands/{Verb}{Entity}/`
- **Queries**: `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Queries/Get{Entity}/`
- **Domain events**: `src/Core/Logistics.Domain/Events/`; handlers in `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Events/`
- **Domain specifications** (base + cross-feature): `src/Core/Logistics.Domain/Specifications/`
- **Application specifications** (feature-scoped, e.g. `SearchTrucks`, `GetInvoices`): `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Specifications/`
- **Infrastructure-port interfaces** (implemented by Infrastructure): `src/Core/Logistics.Application.Abstractions/{Domain}/`
- **Application workflow services** (stay in Application): `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Services/`
- **Application constants**: feature-scoped under `Modules/{Module}/{Feature}/Constants/`; cross-module shared in `Modules/Common/Constants/`
- **Service implementations**: `src/Infrastructure/Logistics.Infrastructure.{Module}/`
- **Shared provider HTTP**: `Logistics.Infrastructure.Integrations.Common/HttpClientJsonExtensions.cs` — `TryGetFromJsonAsync` (never throws; logs, returns `default`), used by LoadBoard/FuelCards/Eld. Don't copy it into a new integration. `Integrations.Accounting`'s QBO helpers deliberately **throw** so push errors surface — not duplicates, don't fold in
- **REST controllers**: `src/Presentation/Logistics.API/Controllers/{Feature}Controller.cs`
- **SignalR hubs**: `src/Infrastructure/Logistics.Infrastructure.Communications/SignalR/Hubs/`
- **Hangfire jobs**: `src/Presentation/Logistics.API/Jobs/` — fan out with `TenantJobRunner.ForEachTenantAsync`. Jobs bypass the MediatR pipeline, so `[RequiresFeature]` is inert: check `IFeatureService.IsFeatureEnabledAsync(tenant.Id, feature)` **inside** the body (a job may need part of its work to run unflagged)
- **Machine-readable API errors**: `Result.Fail(message, ErrorCodes.X)` → `errorCode` → frontend `err.error.errorCode`. Mirror new codes in `Logistics.Shared.Models/ErrorCodes.cs` **and** `projects/shared/src/lib/errors/upgrade-handler.ts`. Never encode a code in the message string
- **Webhooks**: `WebhookController.cs` + `Application/Modules/Integrations/Webhooks/Commands/`
- **EF configurations**: `src/Infrastructure/Logistics.Infrastructure.Persistence/Configurations/{Feature}/`
- **Frontend pages**: `src/Client/Logistics.Angular/projects/{portal}/src/app/pages/{feature}/`
  - Portals: `tms-portal` (dispatchers), `customer-portal` (shippers), `admin-portal` (super admin), `website` (marketing)
- **Mobile (driver)**: `src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/`
- **Provider connect dialogs**: ELD / load board / fuel card "add provider" dialogs MUST use `<app-provider-connect-dialog>` (`tms-portal/src/app/shared/components/other/provider-connect-dialog/`) — project the fields, keep the typed form in the feature, reset from `(opened)`.
- **Address inputs**: every form that captures an `Address` value object MUST use `<ui-address-form>` from `@logistics/shared`. Do not build addresses from raw `<input>`s. Country drives the State / Region / Province label (US/CA/AU/MX/DE/NL/etc., see `state-labels.ts`); State stays required across all countries. Reusable server-side validator: `Logistics.Application.Validators.AddressValidator`.

## Operations

### Loads

- Domain: `Entities/Load/`
- Application: `Modules/Operations/Loads/` (Commands, Queries, Services, Specifications, Constants)
- API/UI: `LoadController.cs`, `tms-portal/pages/loads/`

### Trips

- Domain: `Entities/Trip/`
- Application: `Modules/Operations/Trips/Commands/`, `Modules/Operations/Trips/Queries/`
- Infrastructure: `Routing/Optimization/` (trip optimizer)
- API/UI: `TripController.cs`, `tms-portal/pages/trips/`

### Trucks

- Domain: `Entities/Truck.cs`
- Application: `Modules/Operations/Trucks/Commands/`, `Modules/Operations/Trucks/Queries/`
- API/UI: `TruckController.cs`, `tms-portal/pages/trucks/`

### Customers

- Domain: `Entities/Customer.cs`
- Application: `Modules/IdentityAccess/Customers/Commands/`, `Modules/IdentityAccess/Customers/Queries/`
- API/UI: `CustomerController.cs`, `tms-portal/pages/customers/`

### Customer users

- Domain: `Entities/Customer/CustomerUser.cs`
- Application: `Modules/IdentityAccess/CustomerUsers/Commands/`
- API/UI: `CustomerUserController.cs`, `customer-portal/pages/`

### Containers (ISO 6346)

- Domain: `Entities/Container/`
- Application: `Modules/Operations/Containers/Commands/`, `Modules/Operations/Containers/Queries/`
- API/UI: `ContainerController.cs`, `tms-portal/pages/containers/`

### Terminals (UN/LOCODE)

- Domain: `Entities/Terminal/`
- Application: `Modules/Operations/Terminals/Commands/`, `Modules/Operations/Terminals/Queries/`
- API/UI: `tms-portal/pages/terminals/`

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
- API/UI: `StatController.cs`, `ReportController.cs`, `tms-portal/pages/dashboard/`, `pages/reports/`

## AI dispatch

### Dispatch sessions

- Domain: `Entities/AiDispatch/AiDispatchSession.cs`
- Application: `Modules/Integrations/AiDispatch/Commands/`, `Modules/Integrations/AiDispatch/Queries/`
- Infrastructure: `Infrastructure.AI/Services/AiDispatchService.cs`
- API/UI: `AiDispatchController.cs`, `tms-portal/pages/ai-dispatch/`

### Dispatch decisions

- Domain: `Entities/AiDispatch/AiDispatchDecision.cs`
- Application: `Modules/Integrations/AiDispatch/Commands/Approve*`, `Reject*`
- Infrastructure: `Infrastructure.AI/Services/AiDispatchDecisionProcessor.cs`
- API/UI: (under `ai-dispatch/`)

### Tool registry

- Infrastructure: `Infrastructure.AI/Services/AiDispatchToolRegistry.cs`, `Tools/`
- API/UI: shared with `Logistics.McpServer`

### LLM providers

- Infrastructure: `Infrastructure.AI/Providers/` (Anthropic, OpenAI, factory)

### Quota / pricing

- Domain: `Entities/Subscription/SubscriptionPlan.cs` (`WeeklyAiRequestQuota`)
- Application: `Services/AiQuotaService.cs`
- Infrastructure: `Infrastructure.AI/Services/LlmPricing.cs`
- API/UI: (quota bar in `tms-portal/pages/ai-dispatch/`)

### Global AI settings

- Application: `Modules/Platform/AiSettings/` (`GetAiSettings`, `UpdateAiSettings`); `Abstractions/AiDispatch/LlmModelCatalog.cs`
- Infrastructure: `SystemSettingsService` (`Ai.Model`/`Ai.Provider`/`Ai.ExtendedThinking` keys)
- API/UI: `AiSettingsController.cs`, `admin-portal/pages/ai-settings/`

### Background runner

- API/UI: `Jobs/AiDispatchSessionJob.cs`

### MCP server

- API/UI: `Logistics.McpServer/` (uses `AiDispatchToolRegistry`)

## Compliance & safety

### ELD / HOS logs

- Domain: `Entities/Eld/HosLog.cs`, `HosViolation.cs` (`RuleSetCode`), `DriverHosStatus.cs`, `ValueObjects/HosLimits.cs`
- Application: `Modules/Compliance/Eld/Commands/`, `Modules/Compliance/Eld/Queries/GetHosLimits/`, `Modules/Compliance/Eld/Services/RuleSetSelector.cs` (region → FMCSA / EU 561/2006)
- Infrastructure: `Infrastructure.Integrations.Eld/` (Samsara, Motive, TT ELD, Geotab, Demo); local `Common/EldJsonOptions.cs` (per-provider camel/snake casing), plus `TryGetFromJsonAsync` from `Integrations.Common`
- API/UI: `EldController.cs` (incl. `/eld/rules/limits`), `tms-portal/pages/eld/` (`EldRulesService`, region-aware dashboard)

### ELD provider config

- Domain: `Entities/Eld/EldProviderConfiguration.cs`
- Infrastructure: `Infrastructure.Integrations.Eld/` factory
- API/UI: (under `eld/`)

### ELD sync

- Application: `Modules/Compliance/Eld/Commands/ProcessEldWebhook/` (stamps `RuleSetCode` from tenant region)
- Infrastructure: `WebhookSignature.VerifyHmacSha256` (used by Geotab)
- API/UI: `Jobs/EldSyncJob.cs`, webhooks: `/webhooks/eld/{samsara,motive,geotab,tteld}`

### DVIR inspections

- Domain: `Entities/Safety/DvirReport.cs`, `DvirDefect.cs`
- Application: `Modules/Compliance/Dvir/Commands/`, `Modules/Compliance/Dvir/Queries/`
- API/UI: `DvirController.cs`, `tms-portal/pages/safety/`

### Vehicle inspections

- Domain: `Entities/Inspection/VehicleConditionReport.cs`
- Application: `Modules/Compliance/Inspections/Commands/`, `Modules/Compliance/Inspections/Queries/`
- API/UI: `InspectionController.cs` (list/get/create + `GET /inspections/parts`), `VinController.cs` (`GET /vins/{vin}`)

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
- Infrastructure: `Infrastructure.AI/Tools/CheckDispatchEligibilityTool.cs`
- API/UI: `EmployeeController` (extended), `tms-portal/pages/employees/components/driver-licenses-tab/`, mobile `screens/MyLicensesScreen.kt`, `Jobs/LicenseExpiryReminderJob.cs`

### ADR / Hazmat

- Domain: `Truck.AdrEquipment`, `Load.HazmatClass`, `ValueObjects/AdrEquipment.cs`
- Application: `IDispatchEligibilityService` (gates `DispatchLoad`, `BulkDispatchLoads`, `DispatchTrip` handlers)
- API/UI: (under Loads / Trucks pages — hazmat fields on load-form, ADR fieldset on truck-form)

### IFTA reporting

- Domain: `Entities/Ifta/` (`TruckLocationHistory`, `TruckJurisdictionMileage`, `IftaQuarterSnapshot`), `Entities/IftaTaxRate.cs` (master)
- Application: `Modules/Compliance/Ifta/` (`TruckLocationRecorder`, `IftaReportService`, quarter-close + filing-reminder services, `IftaQuarters`), `Modules/Compliance/Ifta/TaxRates/` (admin rate CRUD), `Modules/Platform/Reports/Queries/Ifta/`
- Infrastructure: `Infrastructure.Routing/Geospatial/JurisdictionResolver.cs` (NTS + embedded boundaries, built once at startup by `JurisdictionResolverWarmup`); `Infrastructure.Documents/Pdf/Ifta/`
- **Traps**: closed quarters are served verbatim from `IftaQuarterSnapshot.ReportJson` (sole source of truth, deliberately not denormalized into columns). Tax-rate `(Year, Quarter, Jurisdiction)` uniqueness is enforced ONLY by `IftaTaxRateUniqueness.FindConflictAsync` — complex-type members can't be in a DB unique index, so every write path must probe or a duplicate silently changes computed tax due
- API/UI: `ReportController` `/reports/ifta` + `/reports/ifta/pdf`, `IftaTaxRateController.cs` `/ifta/rates`, `Jobs/IftaQuarterCloseJob.cs`, `Jobs/IftaFilingReminderJob.cs`, `tms-portal/pages/reports/ifta-report/`, `admin-portal/pages/ifta-rates/`

### Privacy (GDPR)

- Domain: `Entities/Privacy/`
- Application: `Modules/Compliance/Privacy/` (Commands, Queries, Services)
- Infrastructure: `Infrastructure.Documents/Privacy/`, `Infrastructure.Persistence/Privacy/DataAnonymizer.cs`
- API/UI: `PrivacyController.cs`, `Jobs/Data{Export,Deletion,Retention,ExportExpiry}Job.cs`, `<ui-cookie-banner>`, `tms-portal/settings/privacy-settings/`, `admin-portal/data-requests/`

## Financial

### Invoices (TPH)

- Domain: `Entities/Invoice/Invoice.cs` + `LoadInvoice`, `PayrollInvoice`, `SubscriptionInvoice`, `Invoice.RecalculateTotals`, `InvoiceTaxLine`
- Application: `Modules/Financial/Invoices/Commands/`, `Modules/Financial/Invoices/Queries/`, `IInvoiceTaxApplier`
- API/UI: `InvoiceController.cs` + `preview-tax`, `tms-portal/pages/invoices/`, `customer-portal/pages/invoices/`

### Tax engine

- Domain: `Entities/TenantTaxRate.cs`, `Invoice.TaxBehavior`, `Invoice.TaxBreakdownJson`, `ValueObjects/TaxJurisdiction.cs` — build jurisdictions with `TaxJurisdiction.Create(country, region)` on every write path; it upper-cases and normalizes a blank region to null, which is what the equality comparisons rely on
- Application: `Application.Abstractions/Tax/ITaxCalculator`, `Modules/Financial/Tax/Services/IInvoiceTaxApplier`, `Modules/Financial/Tax/Commands/`, `Modules/Financial/Tax/Queries/`, `Modules/Financial/Invoices/Queries/PreviewInvoiceTax/`
- Infrastructure: `Infrastructure.Tax/` (`StripeTaxCalculator`, `ManualTaxCalculator`, `StripeTaxConfigService`, `EuVatRates`/`UsSalesTaxRates`/`OtherCountryRates`); `Infrastructure.Documents/Pdf/Invoice/` VAT block
- API/UI: `TaxController.cs`, `TaxRatesController.cs`, `tms-portal/pages/settings/tax-rates-settings/`, `<ui-money-with-tax>`

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
- API/UI: `StripeConnectController.cs`, `tms-portal/pages/settings/billing/`

### Stripe webhooks

- Application: `Modules/Integrations/Webhooks/Commands/ProcessStripEvent/` (`invoice.paid`, `customer.subscription.*`, `payment_intent.processing|succeeded|payment_failed`, `account.updated`, `checkout.session.completed`)
- Infrastructure: `Infrastructure.Payments/Stripe/`
- API/UI: `WebhookController.cs` (`/webhooks/stripe`)

### Payroll

- Domain: `Entities/Invoice/PayrollInvoice.cs`
- Application: `Modules/Financial/Payroll/` (Commands, Queries, Services)
- API/UI: `tms-portal/pages/payroll/`, `Jobs/PayrollGenerationJob.cs`

### Expenses

- Domain: `Entities/Expense/` (CompanyExpense, TruckExpense, BodyShopExpense)
- Application: `Modules/Financial/Expenses/Commands/`, `Modules/Financial/Expenses/Queries/`
- API/UI: `ExpenseController.cs`, `tms-portal/pages/expenses/`

### Fuel cards

- Domain: `Entities/FuelCards/` (`FuelCardProviderConfiguration`, `FuelCard`, `FuelCardTransaction`); `TruckExpense.PurchaseJurisdiction`/`FuelCardProvider`/`ExternalTransactionId`
- Application: `Modules/Integrations/FuelCards/` (Commands, Queries, `Services/FuelCardSyncService`)
- Infrastructure: `Infrastructure.Integrations.FuelCards/` (WEX, EFS, Demo; factory). WEX/EFS share `Providers/BearerFuelCardService.cs`; config binds once from the `FuelCards` section into aggregate `FuelCardsOptions` (nullable `Wex`/`Efs`, read as `options.Value.Wex ?? new WexOptions()`) — same shape as `LoadBoardOptions`/`EldOptions`
- API/UI: `FuelCardsController.cs` (providers, sync, transaction review queue), `Jobs/FuelCardSyncJob.cs`, `tms-portal/pages/fuel-cards/`
- **Trap**: `FuelCard` has no endpoints by design — it's an internal card→truck mapping table written by `FuelCardSyncService` and `AssignFuelCardTransactionTruckHandler`. It looks orphaned; deleting it breaks both

### Accounting sync (QBO)

- Domain: `Entities/Accounting/` (`AccountingProviderConfiguration`, `QboEntityMapping`)
- Application: `Modules/Integrations/Accounting/` (Commands: Connect/DisconnectQuickBooks; Queries: GetQuickBooksConnection/AuthUrl), `Application.Abstractions/Accounting/` (`IAccountingProviderService`, `IAccountingProviderFactory`, `IOAuthStateProtector`)
- Infrastructure: `Infrastructure.Integrations.Accounting/` (QBO OAuth2 + REST push: Customer/Invoice/Payment/Purchase); token cols encrypted via `Persistence/Converters/EncryptedStringConverter`
- API/UI: `AccountingController.cs` (+ `/accounting/quickbooks/callback`), `Jobs/AccountingSyncJob.cs`, `tms-portal/pages/settings/accounting-settings/`

## Communication

### Real-time messaging

- Domain: `Entities/Messaging/Conversation.cs`, `Message.cs`
- Application: `Modules/Integrations/Messaging/Commands/`, `Modules/Integrations/Messaging/Queries/`
- Infrastructure: `Infrastructure.Communications/SignalR/Hubs/ChatHub.cs`
- API/UI: `MessageController.cs`, `tms-portal/pages/messages/`

### Live tracking

- Application: `Modules/Operations/Tracking/Commands/`, `Modules/Operations/Tracking/Queries/`
- Infrastructure: `Infrastructure.Communications/SignalR/Hubs/TrackingHub.cs`, `Routing/Geocoding/` (Mapbox), `Routing/TripTrackingService.cs`
- API/UI: `TrackingController.cs`, `customer-portal/pages/tracking/`

### Notifications

- Domain: `Entities/Notification.cs`
- Application: `Modules/Platform/Notifications/Queries/`, `Modules/Integrations/UpdateNotifications/Commands/`, `Services/Notification/`
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
- Application: `Modules/IdentityAccess/Users/` (Commands, Queries, Services, Specifications)
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

- API/UI: `Logistics.IdentityServer/` (Duende), all portal `pages/login/`

### Tenants

- Domain: `Entities/Tenant.cs`
- Application: `Modules/IdentityAccess/Tenants/Commands/`, `Modules/IdentityAccess/Tenants/Queries/`, `Services/Tenant/`
- Infrastructure: `Infrastructure.Persistence/Services/Tenant/TenantService.cs`, `TenantDatabaseService.cs`
- API/UI: `TenantController.cs`, `admin-portal/pages/tenants/`

### Impersonation

- Domain: `Entities/ImpersonationToken.cs`, `ImpersonationAuditLog.cs`
- API/UI: `ImpersonationController.cs`

### API keys (MCP)

- Domain: `Entities/ApiKey.cs`
- Application: `Modules/IdentityAccess/ApiKeys/Commands/`, `Modules/IdentityAccess/ApiKeys/Queries/`
- Infrastructure: `Logistics.McpServer/Authentication/`
- API/UI: `ApiKeysController.cs`

### Feature flags

- Domain: `Entities/Feature/DefaultFeatureConfig.cs`, `TenantFeatureConfig.cs`
- Application: `Modules/IdentityAccess/Features/Commands/`, `Modules/IdentityAccess/Features/Queries/`, `Services/Feature/`
- API/UI: `FeaturesController.cs`, `TenantFeaturesController.cs`, `admin-portal/pages/features/`

## Documents & storage

### Documents (POD, BOL, …)

- Domain: `Entities/Document/` (Load, Truck, Employee, Delivery)
- Application: `Modules/Integrations/Documents/Commands/`, `Modules/Integrations/Documents/Queries/`
- Infrastructure: `Infrastructure.Documents/`
- API/UI: `DocumentController.cs`, `customer-portal/pages/documents/`

### PDF generation

- Application: `Services/Pdf/`
- Infrastructure: `Infrastructure.Documents/Pdf/` (QuestPDF: invoices, payroll, BOL, POD)

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

- Domain: `Entities/LoadBoard/LoadBoardListing.cs`, `LoadBoardConfiguration.cs`
- Application: `Modules/Integrations/LoadBoard/Commands/`, `Modules/Integrations/LoadBoard/Queries/`
- Infrastructure: `Infrastructure.Integrations.LoadBoard/` (DAT, Truckstop, 123Loadboard, Demo)
- API/UI: `LoadboardController.cs`, `tms-portal/pages/loadboard/`

### Broker credit

- Domain: `Entities/LoadBoard/BrokerCreditRecord.cs`, `LoadBoardListing.BrokerCredit*`, `TenantSettings.MinBrokerCreditScore`
- Application: `Modules/Integrations/LoadBoard/Queries/GetBrokerCredit/`, booking gate in `Commands/BookLoadBoardLoad/`
- Infrastructure: `Infrastructure.Integrations.LoadBoard/Credit/` (`BrokerCreditService`, `FmcsaClient`); `Infrastructure.AI/Tools/CheckBrokerCreditTool.cs`
- API/UI: `GET /loadboard/brokers/{mc}/credit`, credit badges on `tms-portal/pages/loadboard/`, threshold on `settings/company-settings/`

### Posted trucks

- Domain: `Entities/LoadBoard/PostedTruck.cs`
- Application: `Modules/Integrations/LoadBoard/Commands/PostTruck*`
- API/UI: (under `load-board/`)

### Background sync

- API/UI: `Jobs/LoadBoardSyncJob.cs`

## Settings & integrations

### Tenant settings

- Domain: `Entities/Tenant.cs` → `TenantSettings` VO; `McNumber`/`VatNumber`/`EoriNumber`/`CompanyRegistrationNumber`/`TaxResidencyCountry`
- Application: `Modules/IdentityAccess/Tenants/Commands/UpdateTenant` (regulatory IDs validated server-side via `RegexPatterns.VatNumber`/`McNumber`/`EoriNumber`; `LlmEnabled` toggle per tenant)
- API/UI: `tms-portal/pages/settings/company-settings/`

### System settings

- Domain: `Entities/SystemSettings.cs`
- Application: `ISystemSettingsService.cs`

### Captcha

- Application: `Services/Captcha/`
- Infrastructure: `Infrastructure.Communications/Captcha/` (Google reCAPTCHA)

### Geocoding

- Application: `Services/Geocoding/`
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

## Tests

All under `test/` (singular):

- `Logistics.Application.Tests/` — application handlers, services
- `Logistics.Architecture.Tests/` — layering/dependency boundaries; discovers projects, never hard-code a list
- `Logistics.Infrastructure.AI.Tests/` — AI agent, quota, tools, prompts, pricing
- `Logistics.Infrastructure.{Documents,Payments,Persistence,Routing,Tax,Vin}.Tests/` — per-module

## Updating this map

Add a `### Feature` entry when a top-level feature lands. Don't add sub-features (e.g. _PaymentLink expiration_ belongs under Payment links - the entry already points to the right folders). If a path moves, update the line, don't leave a stale one. Only list the layers a feature actually touches.
