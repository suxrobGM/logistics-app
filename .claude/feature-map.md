# Feature Map

Stable starting points for finding code by feature. Read this before grepping. If a feature isn't here, it doesn't exist yet (or this file is stale - flag it).

## Where things go (conventions)

- **Domain entity**: `src/Core/Logistics.Domain/Entities/{Feature}/`
- **Commands**: `src/Core/Logistics.Application/Commands/{Feature}/{Verb}{Entity}/`
- **Queries**: `src/Core/Logistics.Application/Queries/{Feature}/Get{Entity}/`
- **Domain events**: `src/Core/Logistics.Domain/Events/`; handlers in `src/Core/Logistics.Application/Events/`
- **Specifications**: `src/Core/Logistics.Domain/Specifications/`
- **Service interfaces**: `src/Core/Logistics.Application/Services/{Feature}/` or `src/Core/Logistics.Application.Contracts/Services/`
- **Service implementations**: `src/Infrastructure/Logistics.Infrastructure.{Module}/`
- **REST controllers**: `src/Presentation/Logistics.API/Controllers/{Feature}Controller.cs`
- **SignalR hubs**: `src/Infrastructure/Logistics.Infrastructure.Communications/SignalR/Hubs/`
- **Hangfire jobs**: `src/Presentation/Logistics.API/Jobs/`
- **Webhooks**: `WebhookController.cs` + `Application/Commands/Webhooks/`
- **EF configurations**: `src/Infrastructure/Logistics.Infrastructure.Persistence/Configurations/{Feature}/`
- **Frontend pages**: `src/Client/Logistics.Angular/projects/{portal}/src/app/pages/{feature}/`
  - Portals: `tms-portal` (dispatchers), `customer-portal` (shippers), `admin-portal` (super admin), `website` (marketing)
- **Mobile (driver)**: `src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/`

## Operations

| Feature               | Domain                              | Application                                         | Infrastructure                           | API / UI                                                                                    |
| --------------------- | ----------------------------------- | --------------------------------------------------- | ---------------------------------------- | ------------------------------------------------------------------------------------------- |
| Loads                 | `Entities/Load/`                    | `Commands/Load/`, `Queries/Load/`, `Services/Load/` | -                                        | `LoadController.cs`, `tms-portal/pages/loads/`                                              |
| Trips                 | `Entities/Trip/`                    | `Commands/Trip/`, `Queries/Trip/`                   | `Routing/Optimization/` (trip optimizer) | `TripController.cs`, `tms-portal/pages/trips/`                                              |
| Trucks                | `Entities/Truck.cs`                 | `Commands/Truck/`, `Queries/Truck/`                 | -                                        | `TruckController.cs`, `tms-portal/pages/trucks/`                                            |
| Customers             | `Entities/Customer.cs`              | `Commands/Customer/`, `Queries/Customer/`           | -                                        | `CustomerController.cs`, `tms-portal/pages/customers/`                                      |
| Customer users        | `Entities/Customer/CustomerUser.cs` | `Commands/CustomerUser/`                            | -                                        | `CustomerUserController.cs`, `customer-portal/pages/`                                       |
| Containers (ISO 6346) | `Entities/Container/`               | `Commands/Container/`, `Queries/Container/`         | -                                        | `ContainerController.cs`, `tms-portal/pages/containers/`                                    |
| Terminals (UN/LOCODE) | `Entities/Terminal/`                | `Commands/Terminal/`, `Queries/Terminal/`           | -                                        | `tms-portal/pages/terminals/`                                                               |
| Employees / Drivers   | `Entities/Employee.cs`              | `Commands/Employee/`, `Queries/Employee/`           | -                                        | `EmployeeController.cs`, `DriverController.cs`, `tms-portal/pages/employees/`               |
| Time entries          | `Entities/TimeEntry.cs`             | `Commands/TimeEntry/`, `Queries/TimeEntry/`         | -                                        | `TimeEntryController.cs`, `tms-portal/pages/timesheets/`                                    |
| Dashboard / stats     | -                                   | `Queries/Stats/`, `Queries/Reports/`                | -                                        | `StatController.cs`, `ReportController.cs`, `tms-portal/pages/dashboard/`, `pages/reports/` |

## AI dispatch

| Feature            | Domain                                                                                   | Application                               | Infrastructure                                                 | API / UI                                                      |
| ------------------ | ---------------------------------------------------------------------------------------- | ----------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------- |
| Dispatch sessions  | `Entities/Dispatch/DispatchSession.cs`                                                   | `Commands/Dispatch/`, `Queries/Dispatch/` | `Infrastructure.AI/Services/DispatchAgentService.cs`           | `DispatchAgentController.cs`, `tms-portal/pages/ai-dispatch/` |
| Dispatch decisions | `Entities/Dispatch/DispatchDecision.cs`                                                  | `Commands/Dispatch/Approve*`, `Reject*`   | `Infrastructure.AI/Services/DispatchDecisionProcessor.cs`      | (under `ai-dispatch/`)                                        |
| Tool registry      | -                                                                                        | -                                         | `Infrastructure.AI/Services/DispatchToolRegistry.cs`, `Tools/` | Shared with `Logistics.McpServer`                             |
| LLM providers      | -                                                                                        | -                                         | `Infrastructure.AI/Providers/` (Anthropic, OpenAI, factory)    | -                                                             |
| Quota / pricing    | `Entities/Subscription/SubscriptionPlan.cs` (`AllowedModelTier`, `WeeklyAiRequestQuota`) | `Services/AiQuotaService.cs`              | `Infrastructure.AI/Services/LlmPricing.cs`                     | `tms-portal/pages/settings/ai-settings/`                      |
| Background runner  | -                                                                                        | -                                         | -                                                              | `Jobs/DispatchAgentSessionJob.cs`                             |
| MCP server         | -                                                                                        | -                                         | -                                                              | `Logistics.McpServer/` (uses `DispatchToolRegistry`)          |

## Compliance & safety

| Feature             | Domain                                                            | Application                                      | Infrastructure                                             | API / UI                                                                                      |
| ------------------- | ----------------------------------------------------------------- | ------------------------------------------------ | ---------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| ELD / HOS logs      | `Entities/Eld/HosLog.cs`, `HosViolation.cs`, `DriverHosStatus.cs` | `Commands/Eld/`, `Queries/Eld/`, `Services/Eld/` | `Infrastructure.Integrations.Eld/` (Samsara, Motive, Demo) | `EldController.cs`, `tms-portal/pages/eld/`                                                   |
| ELD provider config | `Entities/Eld/EldProviderConfiguration.cs`                        | -                                                | `Infrastructure.Integrations.Eld/` factory                 | (under `eld/`)                                                                                |
| ELD sync            | -                                                                 | -                                                | -                                                          | `Jobs/EldSyncJob.cs`, webhooks: `/webhooks/eld/*`                                             |
| DVIR inspections    | `Entities/Safety/DvirReport.cs`, `DvirDefect.cs`                  | `Commands/Dvir/`, `Queries/Dvir/`                | -                                                          | `DvirController.cs`, `tms-portal/pages/safety/`                                               |
| Vehicle inspections | `Entities/Inspection/VehicleConditionReport.cs`                   | `Commands/Inspection/`, `Queries/Inspection/`    | -                                                          | `InspectionController.cs`                                                                     |
| Accidents           | `Entities/Safety/AccidentReport.cs`                               | `Commands/Accident/`, `Queries/Accident/`        | -                                                          | `AccidentController.cs`, `tms-portal/pages/safety/`                                           |
| Driver behavior     | `Entities/Safety/DriverBehaviorEvent.cs`                          | `Commands/Safety/`, `Services/Safety/`           | -                                                          | `DriverBehaviorController.cs`                                                                 |
| Maintenance         | `Entities/Maintenance/`                                           | `Commands/Maintenance/`, `Queries/Maintenance/`  | -                                                          | `MaintenanceController.cs`, `tms-portal/pages/maintenance/`, `Jobs/MaintenanceReminderJob.cs` |

## Financial

| Feature              | Domain                                                                                 | Application                                                  | Infrastructure                                                | API / UI                                                                                           |
| -------------------- | -------------------------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Invoices (TPH)       | `Entities/Invoice/Invoice.cs` + `LoadInvoice`, `PayrollInvoice`, `SubscriptionInvoice` | `Commands/Invoice/`, `Queries/Invoice/`                      | -                                                             | `InvoiceController.cs`, `tms-portal/pages/invoices/`, `customer-portal/pages/invoices/`            |
| Payments             | `Entities/Payment/Payment.cs`                                                          | `Commands/Payment/`, `Queries/Payment/`                      | `Infrastructure.Payments/Stripe/StripePaymentService.cs`      | `PaymentController.cs`, `PublicPaymentController.cs`                                               |
| Payment links        | `Entities/Payment/PaymentLink.cs`                                                      | `Commands/PaymentLink/`, `Queries/PaymentLink/`              | -                                                             | `customer-portal/pages/payment/`                                                                   |
| Stripe subscriptions | `Entities/Subscription/Subscription.cs`                                                | `Commands/Subscription/`, `Queries/Subscription/`            | `Infrastructure.Payments/Stripe/StripeSubscriptionService.cs` | `SubscriptionController.cs`, `admin-portal/pages/subscriptions/`, `tms-portal/pages/subscription/` |
| Subscription plans   | `Entities/Subscription/SubscriptionPlan.cs`, `PlanFeature.cs`                          | `Queries/Subscription/`                                      | `Infrastructure.Payments/Stripe/StripePlanService.cs`         | `admin-portal/pages/plans/`                                                                        |
| Stripe Connect       | `Entities/Tenant.cs` (`StripeConnectedAccountId`)                                      | `Commands/StripeConnect/`, `Queries/StripeConnect/`          | `Infrastructure.Payments/Stripe/StripeConnectService.cs`      | `StripeConnectController.cs`, `tms-portal/pages/settings/billing/`                                 |
| Stripe webhooks      | -                                                                                      | `Commands/Webhooks/`                                         | `Infrastructure.Payments/Stripe/`                             | `WebhookController.cs` (`/webhooks/stripe`)                                                        |
| Payroll              | `Entities/Invoice/PayrollInvoice.cs`                                                   | `Commands/Payroll/`, `Queries/Payroll/`, `Services/Payroll/` | -                                                             | `tms-portal/pages/payroll/`, `Jobs/PayrollGenerationJob.cs`                                        |
| Expenses             | `Entities/Expense/` (CompanyExpense, TruckExpense, BodyShopExpense)                    | `Commands/Expense/`, `Queries/Expense/`                      | -                                                             | `ExpenseController.cs`, `tms-portal/pages/expenses/`                                               |

## Communication

| Feature               | Domain                                             | Application                                                                           | Infrastructure                                                                                                               | API / UI                                                       |
| --------------------- | -------------------------------------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Real-time messaging   | `Entities/Messaging/Conversation.cs`, `Message.cs` | `Commands/Messaging/`, `Queries/Messaging/`                                           | `Infrastructure.Communications/SignalR/Hubs/ChatHub.cs`                                                                      | `MessageController.cs`, `tms-portal/pages/messages/`           |
| Live tracking         | -                                                  | `Commands/Tracking/`, `Queries/Tracking/`                                             | `Infrastructure.Communications/SignalR/Hubs/TrackingHub.cs`, `Routing/Geocoding/` (Mapbox), `Routing/TripTrackingService.cs` | `TrackingController.cs`, `customer-portal/pages/tracking/`     |
| Notifications         | `Entities/Notification.cs`                         | `Queries/GetNotifications/`, `Commands/UpdateNotification/`, `Services/Notification/` | `Infrastructure.Communications/Notifications/` (Firebase push), `SignalR/Hubs/NotificationHub.cs`                            | `NotificationController.cs`, `tms-portal/pages/notifications/` |
| Email                 | -                                                  | -                                                                                     | `Infrastructure.Communications/Email/` (Resend, Fluid templates)                                                             | -                                                              |
| Public tracking links | `Entities/TrackingLink.cs`                         | `Commands/Tracking/`                                                                  | -                                                                                                                            | -                                                              |

## Identity & access

| Feature             | Domain                                                               | Application                                                  | Infrastructure                                                                            | API / UI                                                                               |
| ------------------- | -------------------------------------------------------------------- | ------------------------------------------------------------ | ----------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Users               | `Entities/User.cs`, `UserTenantAccess.cs`                            | `Commands/User/`, `Queries/User/`, `Services/User/`          | -                                                                                         | `UserController.cs`, `admin-portal/pages/users/`, `tms-portal/pages/settings/`         |
| Roles / permissions | `Entities/Role/` (AppRole, TenantRole, claims)                       | `Queries/Roles/`                                             | -                                                                                         | `RoleController.cs`                                                                    |
| Invitations         | `Entities/Invitation.cs`                                             | `Commands/Invitation/`                                       | -                                                                                         | `InvitationController.cs`                                                              |
| Auth / OAuth2       | -                                                                    | -                                                            | -                                                                                         | `Logistics.IdentityServer/` (Duende), all portal `pages/login/`                        |
| Tenants             | `Entities/Tenant.cs`                                                 | `Commands/Tenant/`, `Queries/Tenant/`, `Services/Tenant/`    | `Infrastructure.Persistence/Services/Tenant/TenantService.cs`, `TenantDatabaseService.cs` | `TenantController.cs`, `admin-portal/pages/tenants/`                                   |
| Impersonation       | `Entities/ImpersonationToken.cs`, `ImpersonationAuditLog.cs`         | -                                                            | -                                                                                         | `ImpersonationController.cs`                                                           |
| API keys (MCP)      | `Entities/ApiKey.cs`                                                 | `Commands/ApiKey/`, `Queries/ApiKey/`                        | `Logistics.McpServer/Authentication/`                                                     | `ApiKeysController.cs`                                                                 |
| Feature flags       | `Entities/Feature/DefaultFeatureConfig.cs`, `TenantFeatureConfig.cs` | `Commands/Feature/`, `Queries/Feature/`, `Services/Feature/` | -                                                                                         | `FeaturesController.cs`, `TenantFeaturesController.cs`, `admin-portal/pages/features/` |

## Documents & storage

| Feature                 | Domain                                                 | Application                               | Infrastructure                                                          | API / UI                                                          |
| ----------------------- | ------------------------------------------------------ | ----------------------------------------- | ----------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Documents (POD, BOL, …) | `Entities/Document/` (Load, Truck, Employee, Delivery) | `Commands/Document/`, `Queries/Document/` | `Infrastructure.Documents/`                                             | `DocumentController.cs`, `customer-portal/pages/documents/`       |
| PDF generation          | -                                                      | `Services/Pdf/`                           | `Infrastructure.Documents/Pdf/` (QuestPDF: invoices, payroll, BOL, POD) | -                                                                 |
| PDF import / extraction | -                                                      | `Services/PdfImport/`                     | `Infrastructure.Documents/PdfImport/`                                   | -                                                                 |
| VIN decoder             | -                                                      | `IVinDecoderService.cs`                   | `Infrastructure.Documents/Vin/` (NHTSA API)                             | -                                                                 |
| Blob storage            | -                                                      | `IBlobStorageService.cs`                  | `Infrastructure.Storage/Providers/` (Azure, Cloudflare R2, file system) | Selected via `BlobStorage:Type` config (`azure` / `r2` / default) |

## Load board

| Feature           | Domain                                                                | Application                                                        | Infrastructure                                                                | API / UI                                                 |
| ----------------- | --------------------------------------------------------------------- | ------------------------------------------------------------------ | ----------------------------------------------------------------------------- | -------------------------------------------------------- |
| Load board search | `Entities/LoadBoard/LoadBoardListing.cs`, `LoadBoardConfiguration.cs` | `Commands/LoadBoard/`, `Queries/LoadBoard/`, `Services/LoadBoard/` | `Infrastructure.Integrations.LoadBoard/` (DAT, Truckstop, 123Loadboard, Demo) | `LoadboardController.cs`, `tms-portal/pages/load-board/` |
| Posted trucks     | `Entities/LoadBoard/PostedTruck.cs`                                   | `Commands/LoadBoard/PostTruck*`                                    | -                                                                             | (under `load-board/`)                                    |
| Background sync   | -                                                                     | -                                                                  | -                                                                             | `Jobs/LoadBoardSyncJob.cs`                               |

## Settings & integrations

| Feature           | Domain                                     | Application                                     | Infrastructure                                                               | API / UI                     |
| ----------------- | ------------------------------------------ | ----------------------------------------------- | ---------------------------------------------------------------------------- | ---------------------------- |
| Tenant settings   | `Entities/Tenant.cs` → `TenantSettings` VO | `Commands/Tenant/UpdateTenantAiSettings*`, etc. | -                                                                            | `tms-portal/pages/settings/` |
| System settings   | `Entities/SystemSetting.cs`                | `ISystemSettingService.cs`                      | -                                                                            | -                            |
| Captcha           | -                                          | `Services/Captcha/`                             | `Infrastructure.Communications/Captcha/` (Google reCAPTCHA)                  | -                            |
| Geocoding         | -                                          | `Services/Geocoding/`                           | `Infrastructure.Routing/Geocoding/` (Mapbox)                                 | -                            |
| Trip optimization | -                                          | `ITripOptimizer.cs`                             | `Infrastructure.Routing/Optimization/` (heuristic, Mapbox Matrix, composite) | -                            |

## Marketing site

| Feature       | Domain                                              | Application                                       | Infrastructure | API / UI                                                              |
| ------------- | --------------------------------------------------- | ------------------------------------------------- | -------------- | --------------------------------------------------------------------- |
| Blog          | `Entities/BlogPost.cs`                              | `Commands/BlogPost/`, `Queries/BlogPost/`         | -              | `BlogPostController.cs`, `admin-portal/pages/blog-posts/`, `website/` |
| Contact form  | `Entities/ContactSubmission.cs`                     | `Commands/Contact/`, `Queries/ContactSubmission/` | -              | `ContactController.cs`, `admin-portal/pages/contact-submissions/`     |
| Demo requests | `Entities/DemoRequest.cs`                           | `Commands/DemoRequest/`, `Queries/DemoRequest/`   | -              | `DemoRequestController.cs`, `admin-portal/pages/demo-requests/`       |
| Telegram bot  | `Entities/TelegramChat.cs`, `TelegramLoginState.cs` | -                                                 | -              | `Logistics.TelegramBot/`                                              |

## Tests

| Project                                    | Tests for                                |
| ------------------------------------------ | ---------------------------------------- |
| `tests/Logistics.Application.Tests/`       | Application handlers, services           |
| `tests/Logistics.Infrastructure.AI.Tests/` | AI agent, quota, tools, prompts, pricing |

## Updating this map

Add a row when a top-level feature lands. Don't add sub-features (e.g. _PaymentLink expiration_ belongs under Payment links - the row already points to the right folders). If a path moves, update the row, don't leave a stale one.
