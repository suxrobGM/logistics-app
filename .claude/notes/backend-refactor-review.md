# Backend refactor review (2026-11-12)

A read-only review of `src/` and `test/`, about 115k lines in total. Findings marked ✅ were checked against the code. The rest come from the review pass and need a quick check before you act on them. Tick items off as they land.

Baseline: 291 static classes, 441 handlers, 148 validators, 15 infrastructure projects, 12 test projects, ~1,590 test methods.

---

## 0. Bugs found along the way (fix first)

- [x] ✅ **Domain events dispatch _before_ the INSERT.** `DispatchDomainEventsInterceptor` overrides `SavingChangesAsync`, but its comment says "after the save". `Load.Number` and `Trip.Number` are identity columns, so `LoadFactory.Create` (`LoadFactory.cs:64`) and `TripFactory` capture `Number = 0` in the event. The "New load #0" push goes to the driver. Fix: dispatch in `SavedChangesAsync`, or let handlers read the number from the entity.
- [x] ✅ **`LoadComparer` compares `Number + Name`** (`Trip.Stops.cs:16`). Before the save every number is 0, so two new loads with the same name collapse into one in `GetLoads()` and `RecalculateDistance()`. Replace it with `.DistinctBy(l => l.Id)` and delete the file.
- [x] ✅ **`BatchApprovePayrollHandler` sets the fields by hand** (lines 34-41) instead of calling `PayrollInvoice.Approve()`. As a result `PayrollApprovedEvent` never fires for batch approvals: no push, email or Telegram. Move the `PendingApproval` guard into `Approve()` and use it from both handlers.
- [x] ✅ **Stripe cents are truncated, not rounded.** `(long)(x * 100)` in `StripePaymentService.cs:51` and `StripeConnectService.cs:153`, and `plan.Price * 100` in `StripePlanService`. This is wrong for 19.999, and 100× off for zero-decimal currencies (JPY). `Tax/Stripe/StripeMoneyConversion.cs` already gets this right, so use it everywhere.
- [x] ✅ **The client chooses the reviewer id.** `DismissDvirReportRequest(Guid DismissedById, …)` and `RejectDvirReportRequest(Guid RejectedById, …)` (`DvirController.cs:151-152`), and `ReviewAccidentReportRequest(Guid ReviewedById, …)`. Read the id from the claims, the way `InspectionController` does.
- [x] ✅ **Maintenance `PartsCost` double-counts.** `CreateMaintenanceRecordHandler` sets `PartsCost = req.PartsCost`, then adds each part's total on top. Update ignores `Parts`, and there is no validator.
- [x] **Paging without a tie-breaker.** 8 of 9 `OrderBy(req.OrderBy)` calls use the overload that `QueryableExtensions.cs:40-46` itself warns repeats and drops rows. Only `GetUnassignedLoadsHandler` passes a tie-breaker. (Fixed by the paging extension in §2.)
- [x] **Money formatted with the server culture.** 16 `ToString("C")` / `{x:C}` sites ignore `Money.Currency`, so EU tenants see `$`. Add one `Money.Format()`.
- [x] **Payroll Telegram went to the Driver chat role** (removed; the Submitted message to dispatchers stays). "Payroll #N for $X approved" is broadcast to `TelegramChatRole.Driver`, which looks like one person's pay leaking to every driver chat.
- [x] **Resources not disposed.** `TryGetFromJsonAsync` / `TryPostAsync` (`Integrations.Common`) never dispose the `HttpResponseMessage`.
- [x] **`PayrollService.GenerateAsync` hand-rolls the tenant loop** (`PayrollService.cs:93-120`). One tenant's exception aborts all the others, and no cancellation token is passed. Use `TenantJobRunner.ForEachTenantAsync`.
- [x] **Full-table loads and N+1 queries in stats.** `GetExpenseStatsHandler` (138 lines) loads every expense, runs a `GetByIdAsync` per truck, and applies no filter despite saying "last 12 months". `GetInvoiceDashboardHandler` lazy-loads `Payments` per invoice.
- [ ] **Mappers read lazy navigations: 21 sites** (`EldMapper` `?? entity.Employee?.GetFullName()` ×5, `TruckMapper.MainDriver`, `DvirMapper`, `MessagingMapper`, …). This breaks `mapperly.md`'s rule and causes N+1 queries in list endpoints.

## 1. Dead code (delete)

These were confirmed by a name-usage scan of `src/`, `test/`, `tools/` and the private clients.

- [ ] ✅ `Handlers/UpdateTenantEntityHandler.cs`: 0 subclasses.
- [ ] ✅ `UpdateTripHandler.AttachExistingLoadsAsync` (lines 149-196), plus the commented call and `attachedCount = 0` (lines 89-95). `UpdateTripCommand.AttachedLoadIds` is accepted but silently ignored: either finish the feature or drop the field.
- [ ] ✅ `TenantRoleComparer` (`TenantRole.cs:25`).
- [ ] ✅ Interface members nobody calls:
  - `IStripePaymentService.GetPaymentIntentAsync`
  - `IStripeSubscriptionService.UpdateSubscriptionQuantityAsync`
  - `IRealtimeMessagingService.BroadcastTypingIndicatorAsync` / `UserJoined` / `UserLeft`
  - `ILoadService.DeleteLoadAsync`
  - `IUnitOfWork.CommitTransactionAsync` / `RollbackTransactionAsync`
- [ ] ✅ Entity and value-object methods nobody calls:
  - `AuditableEntity.SetCreated` / `SetUpdated`
  - `Load.CanTransitionTo`, `Container.CanTransitionTo`
  - `GetAllowedTransitions` (all 3 state machines), `TripStatusMachine.ValidateTransition` and its whole transition table
  - `DriverHosStatus.GetDrivingTimeRemainingDisplay` / `GetOnDutyTimeRemainingDisplay`
  - `Expense.MarkAsPaid`, `Conversation.CreateDirectConversation`, `Trip.CalcDriversShare`
  - `LoadRoute.WithOrigin` / `WithDestination`, `LoadBoardPlace.FromAddress`
- [ ] ✅ Used only by tests (delete the method and its tests, unless they are meant as API): `AgentConversation.BeginTurn` (11 test refs; production now claims the turn with a conditional update), `LlmMessage.FromUser`, `CargoInspectionPartCategoryExtensions.IsValidFor`. Keep `ProductLicenseToken.CreateSigningCredentials`: `tools/Logistics.LicenseIssuer` uses it.
- [ ] ✅ `HttpClientWriteExtensions.TryPostAsJsonAsync` / `TryPutAsync` / `TryDeleteAsync` and `HttpJsonResult` have 0 callers (~100 lines).
- [ ] ✅ `AppRolePermissions` / `TenantRolePermissions`: `RegisterRole` and `HasRegisteredPermissions` have 0 callers. The `Func` dictionaries exist only to support them.
- [ ] ✅ API `ClaimsPrincipalExtensions.GetRole`, `SeederContext.ResetPerTenantState`, `USStates.FindState`. Shared.Geo `USStates.cs` and `State.cs` are unused, and DbMigrator has its own copy.
- [ ] ✅ `Hangfire.SqlServer` package in `Logistics.API.csproj`: never used. The commented-out MySQL/SQL Server blocks in `Persistence/Utilities/DbContextUtils.cs`: delete them. The file name (`Utils`), class name (`Helpers`) and folder (`Utilities`) also disagree.
- [ ] 5 empty `*ModuleRegistrar.cs` files (only `return services;`): fold Platform's one line into `Registrar.cs`.
- [ ] Empty or useless validators:
  - `RequestDataExportValidator` and `CreateSetupIntentValidator` (0 rules)
  - 13 validators whose only rule is Id-not-empty (forbidden by `csharp-conventions.md`): `DeleteEmployee`, `GetEmployeeById`, `GetUserById`, `GetUserCurrentTenant`, `DeleteDocument`, `GetDriverStats`, `GetRolePermissions`, `CancelTrip`, `DispatchTrip`, `GetTrip`, `GetTripTimeline`, `UpdateLoadProximity`, `ProcessPayment`
- [ ] `ToDictionary()` on `PagedQuery` / `SearchableQuery` / `IntervalQuery`: 0 callers.
- [ ] IdentityServer 2FA: nothing can ever enable 2FA (there is no EnableAuthenticator page and no `SetTwoFactorEnabledAsync(true)`). `TwoFactorAuthentication`, `Disable2fa`, `GenerateRecoveryCodes`, `ShowRecoveryCodes`, `LoginWith2fa` and `LoginWithRecoveryCode` are unreachable (~12 files, ~530 lines). Decide whether to build EnableAuthenticator or delete these pages along with the Angular nav link.
- [ ] Git branches:
  - Merged into main and deletable: local `feat/open-identityserver`, `feat/pilot-readiness`, `feat/replace-mediatr`, `feat/teaser-v2`, `feat/website-lead-emails`; remote `origin/feat/open-identityserver`, `origin/feat/replace-mediatr`, `origin/local` (2022).
  - Not merged by ancestry: `origin/feat/ai-rate-negotiation` (39 commits), `origin/feat/commercial-licensing` (12), and 8 `fork/*` branches. Both features are on main, so these were probably squash-merged. Check with `git diff origin/main...<branch> --stat` before deleting.

## 2. Duplication and hand-rolled code

### Application
- [ ] **`CreateLoadParameters`: 35 positional params, with the field list copied 4×.** The copies are in `CreateLoadHandler`, `CreateTripLoadCommandExtensions`, `LoadService:116-146` and `CreateTripLoadCommand`. Extract a `LoadDetails` base with the shared properties for both commands, and have `NewLoad(LoadDetails, TruckId?, TripId?)` replace the record (~150 lines, 1 file).
- [x] **Paging boilerplate in 14 handlers** (19 converted to `ToPagedResultAsync`; `GetInvoices` waits for the specification removal) (`Count`, `OrderBy`, `ApplyPaging`, `ToArray`, `PagedResult.Ok`). 6 of them are synchronous wrapped in `Task.FromResult`. Add one `ToPagedResultAsync(req, map, ct)` extension with an `Id` tie-breaker (~80 lines; also fixes the paging bug).
- [ ] **Invoice specifications** `FilterLoadInvoices` (12 params) and `FilterPayrollInvoices` (9). They hand-roll `Expression.Invoke` combining (duplicating `ExpressionExtensions.AndAlso`), and each is built twice per request. Only 4 `BaseSpecification` subclasses exist, against 211 `.Query()` uses. Delete `ISpecification` / `BaseSpecification` / `ApplySpecification` (`AddInclude` breaks the lazy-loading rule anyway) and use `IQueryable` extension methods (~350 lines total).
- [ ] **Notification fan-out copy-pasted across 8 event handlers** (4 payroll, 3 load, 1 trip): push, then in-app, then Telegram inside try/catch. Add one injected `EmployeeNotificationSender`. The payroll events repeat 7 payload fields. Slim them to `PayrollId` plus the field specific to each action (~350 lines).
- [ ] **ProcessStripEventHandler** (405 lines, with "Strip" a typo). It pulls the Stripe SDK into Application and is the reason **two architecture tests are skipped** (`BoundaryTests.cs:25`, `CsprojReferenceTests.cs:16`). It switches over 8 event types across 3 modules, and its two payment-intent methods are ~80% identical. Move parsing into `Infrastructure.Payments` behind `IStripeWebhookParser`, which returns plain records. Route those to `Subscriptions` / `Payments` / `StripeConnect` services, rename the handler, and add `StripeMetadataKeys.InvoiceId` / `PaymentLinkToken`. Also fixes M5: the ports return `Stripe.Account` / `Session` / `Customer` / `Subscription`, so Abstractions references Stripe.net.
- [ ] **AI decision approve: copilot vs dispatch.** The copilot handler has 10 ctor deps and dispatch has 8, with identical bodies. `IAgentDecisionAuthorization` and `IAgentDecisionExecution` each have one caller pair. Merge them into one `AgentDecisionApprover`. The "AI enabled" check is written 3×, and copilot *reject* skips it. Don't merge the AICopilot/AIDispatch command twins themselves: `[RequiresFeature]` is any-of.
- [ ] **User creation repeated 4×** (`AcceptInvitation` ×2, `CreateTenant`, `DriverImporter`). The `IdentityResult` error join is written 7×. Add `IUserService.CreateUserAsync`, an `Invitation.Accept()` entity method, and an `Employee` factory that takes the role.
- [ ] **Invitation and welcome emails built 6×** (the accept URL 4×, and two near-identical welcome handlers). Add `IInvitationEmailSender` and `ITenantWelcomeEmailSender`.
- [ ] **Public tracking- and payment-link lookup duplicated 5×.** Add `FindValidAsync(tenantId, token)` returning `Result<…>`. Payment-link creation is duplicated in `SendInvoice` and `CreatePaymentLink`, and `PaymentLinkMapper.cs:24` builds a URL without the tenant id that the handler then overwrites. Add a `PaymentLinkService`.
- [ ] **Invoice editability rules repeated 6×.** Add `Invoice.EditBlockedReason()` / `PaymentBlockedReason(amount)` / `AddLineItem()`, following the existing `TruckChangeBlockedReason()` pattern.
- [ ] **Uniqueness checks in 5 Create/Update pairs** (truck number compared with `ToLower`, trailer number exact). Add UoW extension methods such as `IsTruckNumberTakenAsync(number, exceptId)`.
- [ ] **Small guards that belong on entities:**
  - truck plan-limit check (`CreateTruck` + `TruckImporter`), as `TruckLimitReason`
  - `Expense.Approve` / `Reject` status guard
  - listing `MarkBooked` / `CheckAvailable`
  - `Trip.AssignToTruck` copying the truck onto every load (written in both Create and Update)
- [ ] **`CreateConditionReportHandler` re-implements document upload** without the cleanup on failure that `DeliveryDocumentService` does, so uploaded files are orphaned. It also returns `ex.Message` and contains a nested ternary. Add a shared `ILoadDocumentUploader`, which also replaces the static `DocumentBlobCleanup`.
- [ ] **`tenant.CompanyName ?? tenant.Name` written 18×.** Add `Tenant.DisplayName`.
- [ ] **4 truncate helpers** (`NegotiationText`, `DecisionHistoryDigest`, `AgentDecisionExecution.Compact`, `ToolErrorSanitizer`). Keep one string extension.
- [ ] **2 PascalCase humanizers** (`EntityName.Humanize` and `EnumExtensions`). Keep one.
- [ ] **`PropertyUpdater`** (66 call sites) is effectively `x ?? prev`. Inline it and delete the class. This also removes the 16 hand-written `if (req.X.HasValue)` lines in `UpdateLoadHandler`.
- [ ] **37 redundant `Repository<T>().Update(entity)` calls** on tracked entities, which mark the whole graph Modified. **30 dropped `CancellationToken`s** in 18 handlers.
- [ ] **`ToEntity` mappings written by hand:** `SearchLoadBoardHandler` (12-field copy plus a 28-line listing map, with `"USD"` hard-coded), `FuelCardSyncService`, `EldSyncService`. Move them to Mapperly.
- [ ] **Tool names as bare strings in prompts and handlers** (`"get_trip"`, `"propose_counter_offer"`, …). Add `AgentToolNames` constants.
- [ ] **Two "turn starter" services repeat the same flow** (append, save, claim, then schedule or queue). Add a shared `AppendAndStartTurnAsync`.

### Domain and mappings
- [ ] **`LoadFactory` / `TripFactory` duplicate `Load.Create` / `Trip.Create`.** `Load.Factory.cs` only forwards 12 params, and `LoadFactory.CreateTripStops` copies `TripFactory.AddStopsForLoads`. Move the bodies onto the entities and delete both static classes (~120 lines).
- [ ] **State machines.** Trip: keep `CanModify` / `CanCancel` / `CanDispatch` as `Trip` properties and delete the file. Load and Container: stamp the timestamps directly in `UpdateStatus` and drop the `LoadStatusTimestamps` / `LoadStatusState` round-trip (~170 lines). No Stateless.
- [ ] **Document factories repeat 6 file fields across 4 `Create` methods** (10-17 params). Add a `StoredFile` record and collapse the 3 branches in `UploadDocumentHandler`.
- [ ] **24 of 55 "mapper" files are hand-written** `new XDto {}` (`LoadMapper.ToDto(LoadLookup)` alone is 70 lines), so a new field is silently left out. Convert them to Mapperly `partial` + `dto with {}`. Set `RequiredMappingStrategy.Target` assembly-wide to drop ~150 `MapperIgnoreSource` lines; keep the ignores on navigations.
- [ ] **`IResult`** has one implementation and clashes with ASP.NET `IResult`. Use `Result`. Keep the hand-rolled `Result`: switching to ErrorOr/FluentResults would touch ~1,000 sites.
- [ ] **`Money` is a record with `set` properties**, and its implicit `decimal` operator drops the currency. Make the properties `init`.
- [ ] **Permissions:** replace the `list.AddRange` chains (`TenantRolePermissions.cs:50-170`) with module arrays plus `SelectMany`. Derive Owner as all permissions minus an exclude list, as Admin already is. `GetBasicPermissions` is appended in 3 places.

### Infrastructure and Presentation
- [ ] **Snake-case conversion written 3×:** `SnakeCaseEnumConverter` (126 lines), `SnakeCaseEnumModelBinder`, and `JsonNamingPolicy.SnakeCaseLower`. Use the BCL policy in the converter, with a parity test over every domain enum so the stored values don't change.
- [ ] **`HasPrecision(18,2)` ×47 and currency `HasMaxLength(3)` ×22.** Replace them with `ConfigureConventions` and verify with an empty migration.
- [ ] **`ProviderFactoryBase` plus 4 `Dictionary<Enum,Type>` maps** can become .NET keyed services (`AddKeyedTransient`, `IServiceProviderIsKeyedService`) (~100 lines, 2 files). Update the `add-integration-provider` skill.
- [ ] **ELD providers:** the base URL is defaulted twice (in options and again with `??`), `DefaultRequestHeaders.Add` is called with no Remove (Motive), and the `ValidateCredentialsAsync` try/catch appears 4×. Set `BaseAddress` once in `AddHttpClient`.
- [ ] **US states listed 3× and EU-27 listed 3×** (tax tables, `TaxJurisdictionsProvider`, `RegionCountries`, `EUVatRules`). Derive them from one source.
- [ ] **`LlmPricing` repeats the model ids in `LlmModelCatalog`,** with a parity test to keep them in step. Put the prices on `LlmModelInfo`.
- [ ] **361 hand-written `IsSuccess ? Ok/NoContent : BadRequest/NotFound` ternaries in controllers.** Add `ErrorCodes.NotFound` and one `ToActionResult()` extension, mainly for consistent 404s. Keep controllers: OpenAPI generation and `ControllerAuthorizationTests` depend on them.
- [ ] **`LoadBoardSyncJob`** holds domain logic and row-by-row deletes. Move the logic to an Application service and use `ExecuteDeleteAsync`. The **10 jobs that are a single `RecurringJob.AddOrUpdate`** line can collapse into one `RecurringJobs.cs` (10 files).
- [ ] **`IdentityServer/Setup.AddAuthSchemes`** hand-registers the 4 Identity cookies; use `AddIdentityCookies()`. **`Pages/Telegram/Login.cshtml.cs`** has a 100-line `OnGet` that writes to both databases; move it into `TelegramAuthService`.
- [ ] **TelegramBot:** the Loads/Trucks/Trips commands repeat the paging-callback and send-or-edit logic (~100 lines). Switching from MarkdownV2 to `ParseMode.Html` would drop most `Escape()` calls and the risk of truncating mid-escape.
- [ ] **Photo `IFormFile` → `FileUpload` loop ×3** (Document and Inspection controllers). Add a `ToFileUploads()` extension.
- [ ] **Seeders:** give `SeederBase.random` a fixed seed (demo data then reproduces). `DemoEldService` uses `string.GetHashCode()`, so demo names change on every restart.

## 3. Large methods and long parameter lists

| Where | Size | Fix |
| --- | --- | --- |
| `CreateLoadParameters` | 35 params | `LoadDetails` (§2) |
| `DeliveryDocument.Create` | 17 params | `StoredFile` (§2) |
| `CreateInspectionRequest` (controller) | 16 params | bind the command directly |
| `PayrollPaid` / `Approved` / `Rejected` / `Submitted` events | 9-13 params | slim the payload (§2) |
| `FilterLoadInvoices` / `FilterPayrollInvoices` | 12 / 9 params | delete the specs (§2) |
| `ApproveAICopilotDecisionHandler` | 10 deps | `AgentDecisionApprover` (§2) |
| `AcceptInvitationHandler.Handle` | ~130 lines | `CreateUserAsync` + `Invitation.Accept` |
| `CreateConditionReportHandler.Handle` | ~110 lines | shared uploader |
| `BookLoadBoardLoad` / `ProposeCounterOffer` | ~140 / 160 lines | listing entity methods, `ICustomerFinder` |
| `GetExpenseStatsHandler` | 138 lines | group on the server |
| `AgentTurnService.RunTurnAsync` | 105 lines | extract the gate check and the persistence step |
| `GetContainerStatusTool`, `MapboxMatrixTripOptimizer`, `FactoringSchedulePdfService` | 94-107 lines | split into named sections |
| `NegotiationSeeder.BuildThreadAsync`, `SubscriptionPlanSeeder.SeedAsync` | 146 / 163 lines | one method per scenario or plan |
| `InvoicesController` | 424 lines | `PayrollInvoicesController` (same routes) |

Partial `.Split` files: `Load` is ~600 lines over 8 partials, each partial covers one area, so **keep them**. Don't move Freight or appointments into complex types until they gain behaviour. Removing the factories and state machines above takes ~280 lines out of `Load` anyway.

## 4. Static classes

Of the 291, most are fine: Mapperly mappers, pure calculators (`DetentionCalculator`, `ContainerFreeTimeCalculator`, `VinFormat`), constants, and extension holders. These ones should change:
- **Should be entity methods:** `LoadFactory`, `TripFactory`, `TripStatusMachine`, and the Load/Container machine round-trip.
- **Should be instance services** (they take services or a UoW): `DocumentBlobCleanup`, `ImportFileReader.Read(ISpreadsheetReader, …)`.
- **Delete:** `PropertyUpdater`, `DateTimeHelpers` (one method; move it into `DateTimeExtensions`).
- **In the wrong project:**
  - `Domain/Options/*` → Abstractions
  - `Domain/Extensions/QueryableExtensions` → Application (drops `System.Linq.Dynamic.Core` from Domain)
  - `PrivacyDefaults` → the Privacy module
  - `HosLimits` (a DTO) → Shared.Models
  - `TruckLoadCompatibility.Rules` (prompt text) → Infra.AI
  - `StripeMoneyConversion` → Payments
  - `StripeObjectMapper` (lives in StripeConnect, used by Subscriptions and Webhooks)

## 5. Layout

**Projects: 32 → ~22.**

| Today | Target |
| --- | --- |
| `Domain`, `Domain.Primitives`, `Shared.Geo` | `Logistics.Domain` (keep the namespaces; only csproj files change) |
| `Application`, `Mappings` | `Logistics.Application`, with mappers next to their features. `NotificationService` receives a DTO |
| `Integrations.{Common,Eld,LoadBoard,FuelCards,Accounting}`, `Vin`, `Licensing` (37 lines) | `Logistics.Infrastructure.Integrations` with folders. Move `VerifySvix` to Communications; Telegram uses `CryptographicOperations.FixedTimeEquals` |
| `Tax` (already references Payments and Stripe) | `Infrastructure.Payments/Tax/` |
| AI, Persistence, Documents, Communications, Routing, Storage | Keep: each brings heavy dependencies of its own |

Update `AssemblyAnchors`, the arch-tests csproj, `overview.md`, CLAUDE.md ("15 focused projects") and the provider skill. Add an arch rule: Shared.Models must not use `Domain.Entities`.

**Inside Application:**
- [ ] CustomerUser queries live in `Platform/Portal/Queries`; move them to `IdentityAccess/CustomerUsers/Queries`.
- [ ] 10 payroll features live in `Financial/Invoices`; move them to `Financial/Payroll`.
- [ ] Put `GetAIQuotaStatusHandler` in `Agents/Queries`. The cross-tenant quota admin features move from AIDispatch to `Platform/AISettings`.
- [ ] `ProcessEldWebhook` lives in `Compliance/Eld`, but the docs say `Integrations/Webhooks`: move it or fix the doc.
- [ ] Merge the two "Common" folders (`Modules/Common` and `Operations/Common`). `LoadLookupBuilder` goes into `Services/`. Rename `VehicleTransportGuard` (jargon noun).
- [ ] Optional: drop the per-command subfolder (158 folders, namespaces already flat). This only helps navigation.

**Recommended against (and why):** Refit or Kiota clients (4-8 endpoints per vendor, with different error styles), `IChatClient` (hides Anthropic cache breakpoints, thinking signatures and PauseTurn), prompts as `.md` resources (loses compile checks and byte-identical cache prefixes), Minimal APIs, Bogus/AutoFixture, Stateless, replacing the UoW (it switches tenant databases), and Scrutor scanning for infrastructure.

**Formatting:** 15+ files have broken indentation left over from the C# 14 `extension` conversion; run `dotnet format`. 8 files have mojibake (`Ã¢â‚¬â€`): `ProcessStripEventHandler`, `DispatchEligibilityService`, the Privacy services, `DispatchLoad`, `DispatchTrip`, and `DispatchEligibilityServiceTests`.

## 6. Tests: ~140 methods and ~3,000 lines removable (~9%)

The suite is less redundant than it looks: most large files test real branching (money, HOS, negotiation floors, agent safety).
- [ ] `AIDispatchSystemPromptTests`: 43 → ~16 tests. Delete ~21 tests that only `Contains`-check prose. Add 2 drift tests: every tool name in the prompt exists in `AgentToolCatalog`, and every back-ticked enum name is real. Keep the sanitization, truncation, ordering and cache-prefix tests.
- [ ] ~30 per-tool "missing or invalid input" tests in AI.Tests: the binding lives in one place (`AgentToolJson.cs:66`) and is already tested there.
- [ ] `AgentDecisionProcessorTests` (−5 tests), `AgentToolRegistryTests` (23 → 17), `AIDispatchConversationBuilderTests` (−5), `HosFeasibilityToolTests` (5 parity tests → 1 Theory).
- [ ] `SnakeCaseEnumConverterTests`: 17 → 4 methods (`Roundtrip_EveryDomainEnumMember` already covers the rest).
- [ ] Add `BrokerCreditCheckerTests` (one Theory). Cut its 9 indirect tests in the two handler test files down to one each.
- [ ] Cross-layer duplicates:
  - `PayrollServiceTests` exclusions duplicate `EmployeePayTests`. These two also use `double` and break the decimal rule.
  - The duplicate hazmat eligibility test.
  - `UpdateLoadHandlerTests`: 4 rebuild tests → 1 Theory.
  - `QboMapperTests`: −5.
  - `QueryableExtensionsTests`, `AIQuotaServiceTests` and `ProposeCounterOfferHandlerTests`: merge into Theories.
- [ ] `ApplicationLayerRegistrationTests` is a hand-maintained `InlineData(typeof(IX))` roster, the same anti-pattern CLAUDE.md bans. Replace it with one `ValidateOnBuild` + `ValidateScopes` test.
- [ ] Delete `AIDispatchSessionMapperTests` (tests generated Mapperly code). Merge the 7 one-test EF mapping files into one `TenantModelMappingTests`.
- [ ] Shared test kit (~1,150 lines): 87 `new Address {…}` blocks, 25 hand-built `Load`s, ~15 hand-built `Tenant`s (`TestTenant` exists, and AI.Tests keeps a copy), and 182 `Repository<T>().Returns` lines. Add `TestData.*` builders and a `uow.Stub<T>(rows)` extension. Turn `DispatchEligibilityServiceTests` into a `[MemberData]` scenario table (518 → ~300 lines).
- [ ] 11 files (~2k lines) in Application.Tests actually test Infrastructure (QBO, Samsara, Geotab, DAT, FMCSA, webhook signatures, fuel cards). Move them.
- [ ] **Test projects: 12 → 5.** Application.Tests (mirror `Modules/`), a new merged `Infrastructure.Tests` (Persistence, Tax, Payments, Vin, Routing, Communications, Documents, Integrations, HostDefaults), AI.Tests, Mediator.Tests, Architecture.Tests.

---

## Suggested order

1. §0 bugs (small, independent commits).
2. §1 dead code: mechanical, lowest risk, biggest "less to read" win.
3. Shared paging extension, `LoadDetails`, notification sender, entity guards (§2 Application).
4. Domain: factories and state machines onto the entities, specifications removed, mappers moved to Mapperly.
5. The Stripe webhook split, which brings back 2 architecture tests.
6. Test trim and test kit, then test-project merge.
7. Project consolidation last (mechanical, but touches the architecture tests, docs and skills).

Rough total: **~5,000 src lines, ~90 files, ~10 projects removed**, plus ~3,000 test lines and 7 test projects.
