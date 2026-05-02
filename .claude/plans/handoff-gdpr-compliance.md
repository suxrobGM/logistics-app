# Handoff: GDPR Compliance — Data Export, Consent, Retention

> **Priority:** HIGH (legal blocker for EU customer data). **Effort:** M (3–5 days).
>
> The codebase has tenant deletion ([DeleteTenantHandler.cs](../../src/Core/Logistics.Application/Commands/Tenant/DeleteTenant/DeleteTenantHandler.cs)) which drops the entire tenant DB, but no per-user data export (Article 15), no cookie consent banner, and no retention policy. All three are mandatory before processing EU personal data.

## Sequencing

- **Position in overall order:** 3rd
- **Depends on:** Nothing. Mostly orthogonal to the rest.
- **Unblocks:** Legal sign-off for EU launch. The cookie banner UX leans on plan #6 ([i18n](handoff-i18n-multi-language.md)) for translation — implement banner now in English, internationalize copy when #6 lands.
- **Why third:** Can run in parallel with plans #1 / #2 (different code paths). Putting it before any user-facing EU traffic ships is non-negotiable; doing it third lets it land at the same time as billing readiness.

## Why now

- GDPR Articles 15 (right of access), 17 (erasure), 20 (portability) require user-level not just tenant-level operations
- ePrivacy Directive requires consent for non-essential cookies on the website / customer portal
- Article 5(1)(e) requires data retention limits — tracking pings, audit logs, dispatch sessions accumulate forever today

## Current state

- [DeleteTenantHandler.cs](../../src/Core/Logistics.Application/Commands/Tenant/DeleteTenant/DeleteTenantHandler.cs) — drops DB + Stripe customer (good for tenant deletion)
- No `User.DeletionRequestedAt` / soft-delete flow
- [website](../../src/Client/Logistics.Angular/projects/website/) — no consent banner, presumably uses analytics/tracking pixels
- [Logistics.Domain/Entities/](../../src/Core/Logistics.Domain/Entities/) — many entities have `CreatedAt` but no `RetainUntil`
- ImpersonationAuditLog and audit trails — verify retention

## Backend

### Domain

- New entity `Entities/Privacy/DataExportRequest`:
  - `UserId`, `RequestedAt`, `Status` (Pending/Processing/Ready/Failed/Expired), `BlobPath`, `ExpiresAt`
- New entity `Entities/Privacy/DataDeletionRequest`:
  - `UserId`, `RequestedAt`, `ScheduledFor` (30-day grace period), `Status`, `Reason`
- Add `User.DeletionRequestedAt` (nullable) and `User.AnonymizedAt` (nullable)
- New enum `DataCategory`: Profile, Loads, TimeEntries, HosLogs, Messages, Notifications, AuditLogs, Documents, Payments
- New entity `Entities/Privacy/ConsentRecord`:
  - `UserId`, `ConsentType` (Marketing, Analytics, Functional), `Granted` (bool), `Timestamp`, `IpAddress`, `UserAgent`

### Application

- New service contract `IDataExportService.GenerateExportAsync(Guid userId, CancellationToken)` → produces a ZIP with per-category JSON files + uploaded documents
- New `Commands/Privacy/RequestDataExport`, `Commands/Privacy/RequestDataDeletion`, `Commands/Privacy/CancelDataDeletion`, `Commands/Privacy/RecordConsent`
- New `Queries/Privacy/GetDataExportRequestQuery`, `GetConsentHistoryQuery`
- Background job `Jobs/DataExportJob` (Hangfire) — picks up Pending exports, generates ZIP, stores in blob, emails user a signed link
- Background job `Jobs/DataDeletionJob` — runs daily, processes deletion requests past their grace period, anonymizes (don't hard-delete operational records — replace name/email/phone with `[deleted]`, keep referential integrity for invoices/loads)
- Background job `Jobs/DataRetentionJob` — daily, purges:
  - Tracking pings older than 90 days
  - Notifications older than 1 year (already-read)
  - Dispatch session transcripts older than 2 years
  - Audit logs older than 7 years (financial regulation overrides GDPR for these)
  - Configurable per-tenant in `TenantSettings` (override defaults)

### Infrastructure

- Implementation `Logistics.Infrastructure.Documents/Privacy/DataExportService.cs`:
  - Iterates `DataCategory`, calls per-category exporter (each is a small mapper from entity → DTO)
  - Uses `IBlobStorageService` to upload the ZIP under `data-exports/{userId}/{requestId}.zip`
  - Generates signed URL with 7-day expiry
- Email template (Fluid) `data-export-ready.liquid`
- `IDataAnonymizer` — replaces PII while keeping FK integrity; tested per entity

### Persistence / migration

- Tenant DB migration: new tables `DataExportRequests`, `DataDeletionRequests`, `ConsentRecords`; add columns to `User`
- Add `RetainUntil` (nullable DateTime) to: `Notification`, `TripTracking` (or whatever tracking entity exists), `DispatchSession`
- Add `TenantSettings.RetentionDays` complex sub-record (TrackingDays, NotificationDays, etc.) — default values

## API

- New `PrivacyController`:
  - `POST /api/privacy/export` (auth: any user, rate-limited 1/day)
  - `GET /api/privacy/export/{id}` — status + signed download link
  - `POST /api/privacy/delete` — schedules deletion
  - `DELETE /api/privacy/delete/{id}` — cancel within grace period
  - `POST /api/privacy/consent` — anonymous endpoint for cookie banner (no auth, validates IP rate limit)
  - `GET /api/privacy/consent/history` — auth required
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- New `<ui-cookie-banner>` component:
  - Categories: Strictly necessary (always on), Functional, Analytics, Marketing
  - Stores accepted state in localStorage AND sends to `/api/privacy/consent`
  - Reads `Do-Not-Track` header and pre-disables analytics
- `services/consent.service.ts` — exposes `hasConsent(category)`; analytics service must check before initializing
- New `<ui-privacy-settings>` component used by all portals' user-settings pages

### Website + Customer portal

- Add `<ui-cookie-banner>` to root layout — fires on first visit, persists choice
- Add `/privacy-policy` and `/cookies` pages (content as placeholder; legal copy provided by lawyers)
- Wrap analytics initialization (Mapbox events, any GA-equivalent) in consent check

### TMS portal + Customer portal

- New page `pages/settings/privacy/`:
  - "Download my data" button → `POST /api/privacy/export`
  - "Delete my account" button → confirmation modal explaining 30-day grace period and what gets anonymized vs preserved (financial records)
  - Consent history table

### Admin portal

- New page `pages/data-requests/` — superadmin view of all pending exports/deletions with manual override
- `pages/tenants/[id]/` — show retention policy config

## Mobile (driver app)

- New screen `screens/PrivacyScreen.kt` — same export/delete buttons as web
- Cookie banner not applicable (native app), but record consent for analytics on first launch

## Tests

- Application tests: each privacy command + retention job
- Anonymizer tests per entity — PII gone, FKs intact, invoice totals unchanged
- E2E: cookie banner workflow (accept analytics → cookie set → reject → cookie cleared)

## Acceptance criteria

- [ ] User clicks "Download my data" → ZIP arrives via email within 5 minutes containing all PII categories
- [ ] User requests deletion → status shows 30-day grace; cancel works; after grace, name/email/phone replaced with `[deleted]` but their loads still belong to the tenant for accounting
- [ ] First visit to website shows cookie banner, declining analytics prevents the analytics SDK from loading
- [ ] Tracking pings older than 90 days are gone after retention job runs
- [ ] Audit logs preserved for 7 years even after deletion (financial obligation)
- [ ] Consent record API rate-limited (no spam)

## Update [.claude/feature-map.md](../feature-map.md)

Add new "Privacy & compliance" section:

| Data export | `Entities/Privacy/DataExportRequest.cs` | `Commands/Privacy/`, `Services/Privacy/IDataExportService` | `Infrastructure.Documents/Privacy/DataExportService.cs`, `Jobs/DataExportJob.cs` | `PrivacyController.cs`, `tms-portal/pages/settings/privacy/` |
| Cookie consent | `Entities/Privacy/ConsentRecord.cs` | `Commands/Privacy/RecordConsent/` | - | `<ui-cookie-banner>`, all portals |
| Retention | `RetainUntil` columns, `TenantSettings.RetentionDays` | - | `Jobs/DataRetentionJob.cs` | `admin-portal/pages/tenants/` |
