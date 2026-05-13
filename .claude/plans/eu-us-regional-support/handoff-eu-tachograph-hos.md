# Handoff: EU Tachograph & EC 561/2006 HOS Rules

> **Status: Done (2026-05-13).** Phase A (region-aware HOS types + display) and Phase C (Geotab tachograph provider) shipped. Phase B from the original handoff (a local rule evaluator) was **rejected, not deferred** — see architectural decision below.

## Architectural decision

**HOS data is always provider-sourced. There is no local rule evaluator.** All duty-status changes, remaining-minutes calculations, and violations come from the connected ELD or tachograph device via its provider API. Reasons:

- The hardware device is the legally certified source of truth in both FMCSA (49 CFR Part 395) and EU 561/2006 / Regulation 165/2014. A locally-computed counter that disagrees with the device is an audit liability.
- Every supported provider already returns pre-computed `DrivingMinutesRemaining` / `OnDutyMinutesRemaining` / `CycleMinutesRemaining` and violation events.
- Tachograph providers (Geotab, Webfleet, Frotcom) do the same for EU drivers.

The `HosLimits` value object exposed via `GET /api/eld/rules/limits` is **for display only** — the UI uses it to render counters and threshold colours. It must never override or compute violations.

Self-reported HOS for non-ELD drivers is **not supported**; drivers without a certified device cannot be tracked by this system.

## What shipped

### Phase A — Region-aware HOS types + display

- **Domain:** [HosViolationType](../../src/Core/Logistics.Domain.Primitives/Enums/HosViolationType.cs) extended with EU 561/2006 values in the 100–199 range (`EuContinuousDriving4_5h`, `EuDailyDriving9h`, `EuWeeklyDriving56h`, `EuBiweeklyDriving90h`, `EuDailyRest11h`, `EuWeeklyRest45h`, `EuFormAndManner`); FMCSA values stay in 1–99.
- **Value object:** [HosLimits](../../src/Core/Logistics.Domain.Primitives/ValueObjects/HosLimits.cs) with `Fmcsa()` / `Eu561_2006()` factories and `FmcsaCode` / `Eu561Code` constants.
- **Persistence:** [HosViolation.RuleSetCode](../../src/Core/Logistics.Domain/Entities/Eld/HosViolation.cs) (string, max 32, default `"FMCSA"`); EF migration `Version_0010` backfills existing rows.
- **Application:** [RuleSetSelector](../../src/Core/Logistics.Application/Services/Hos/RuleSetSelector.cs) maps `Tenant.Settings.Region` to a rule-set code / `HosLimits`. [ProcessEldWebhookHandler](../../src/Core/Logistics.Application/Commands/Eld/ProcessEldWebhook/ProcessEldWebhookHandler.cs) stamps `RuleSetCode` on incoming violations.
- **API:** `GET /api/eld/rules/limits` → [HosLimitsDto](../../src/Shared/Logistics.Shared.Models/Eld/HosLimitsDto.cs).
- **Frontend (shared):** [LocalizationService.formatHosDuration](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts) (`"11h 30m"` US, `"11 h 30 min"` EU); `getPrimeNgDateFormat()` for date pickers.
- **Frontend (TMS):** [EldRulesService](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/core/services/eld-rules.service.ts) caches `/eld/rules/limits` per session. Dashboard binds warn thresholds to 10% / 15% of the active limit; rule-set badge in page header. HOS logs page uses locale-aware date format and duration display.

### Phase C — Geotab tachograph provider

- [Geotab/](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/Geotab/) — `GeotabClient` (MyGeotab JSON-RPC over POST `/apiv1`), `GeotabModels`, `GeotabMapper` (region-aware violation type mapping), `GeotabEldService` (implements `IEldProviderService`).
- Credentials encoded as `ApiKey="database|userName"`, `ApiSecret=password`. Federated server discovery on authenticate.
- [WebhookSignature.VerifyHmacSha256](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/WebhookSignature.cs) — constant-time HMAC verification used by `GeotabEldService.ProcessWebhookAsync`. `POST /webhooks/eld/geotab` reads `X-Geotab-Signature`.
- Replaces `NotImplementedException` in [EldProviderFactory](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/EldProviderFactory.cs).
- TMS portal: Geotab in [ELD_PROVIDER_OPTIONS](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/eld/_components/eld.constants.ts); credentials hint added to provider-add dialog.

### Cross-provider cleanup (incidental)

While in the file, all four provider services were simplified:

- [EldJsonOptions](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/EldJsonOptions.cs) centralises camelCase / snake_case naming policies — model files dropped ~90 `[JsonPropertyName]` attributes.
- [HttpClient.TryGetFromJsonAsync](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/HttpClientEldExtensions.cs) extension wraps GET + status check + parse + log on failure, replacing repeated try/catch blocks across Samsara / Motive / TT ELD / Geotab.

## Tests

- [HosLimitsEndpointTests](../../test/Logistics.Application.Tests/Eld/HosLimitsEndpointTests.cs) — US tenant returns FMCSA limits; EU tenant returns EU 561/2006 limits; `RuleSetSelector` theory cases.
- [GeotabMapperTests](../../test/Logistics.Application.Tests/Eld/GeotabMapperTests.cs) — region-aware violation type mapping (US → FMCSA values 1–99, EU → EU values 100+); duty-status mapping; region isolation invariant.
- [WebhookSignatureTests](../../test/Logistics.Application.Tests/Eld/WebhookSignatureTests.cs) — HMAC verification: valid / uppercase / tampered / wrong secret / missing inputs.
- [GeotabWebhookTests](../../test/Logistics.Application.Tests/Eld/GeotabWebhookTests.cs) — service-level: valid signature parses event, invalid signature rejected before parsing, no-secret-configured falls through, malformed JSON rejected.

40 Eld tests pass.

## Out of scope (intentional)

- **Local HOS rule evaluator** — see Architectural decision above.
- **Mobile HOS UI** — no HOS screens exist in the driver app today; greenfield work, future plan.
- **UK-specific rules** — UK tenants use `Region.EU` until a customer needs divergence; add a `Region.Uk` override when that happens.
- **AETR rule set** — same parameters as EU 561/2006 with minor differences; subclass later if needed.
- **String extraction in templates** — covered by the deferred phase of plan #6 (i18n).
