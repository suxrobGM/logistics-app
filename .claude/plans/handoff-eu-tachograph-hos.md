# Handoff: EU Tachograph & EC 561/2006 HOS Rules

> **Priority:** HIGH (compliance blocker for EU drivers). **Effort:** XL (2–3 weeks; consider phasing).
>
> All ELD providers ([Samsara, Motive, Geotab, TT ELD](../../src/Core/Logistics.Domain.Primitives/Enums/EldProviderType.cs)) target US FMCSA. EU drivers use **digital tachographs** with completely different rules (EC 561/2006: 4.5h driving max, 9h daily, weekly/biweekly limits). Without this, EU dispatchers cannot legally schedule drivers.

## Sequencing

- **Position in overall order:** 10th
- **Depends on:** Plan #6 ([i18n](handoff-i18n-multi-language.md)) — HOS labels and warnings are user-visible and constantly read. Plan #7 ([driver licensing](handoff-driver-licensing-and-adr.md)) — eligibility integration assumes licensing fields exist. Plan #9 ([volume/units](handoff-fuel-volume-temperature-units.md)) is **not** required but tachograph also reports odometer in km — easier if the unit infra is there.
- **Unblocks:** Legal EU dispatching. Nothing else strictly waits on this.
- **Why tenth (last among "high"):** Largest single piece of work. Phase A alone (rule-engine refactor) can ship ahead of Phase B/C — break it across sessions per the in-plan phasing.

## Why now

- HOS violation calculation in [HosViolation.cs](../../src/Core/Logistics.Domain/Entities/Eld/HosViolation.cs) uses FMCSA limits (11h driving, 14h on-duty) hardcoded
- EU `EldProviderConfiguration` has no provider option to configure
- Mobile driver app HOS displays use US terminology ("11-hour limit") — wrong for EU drivers

## Current state

- [EldProviderType.cs](../../src/Core/Logistics.Domain.Primitives/Enums/EldProviderType.cs): Samsara, Motive, Geotab, Omnitracs, PeopleNet, TtEld, Demo — **all US**
- [HosLog.cs](../../src/Core/Logistics.Domain/Entities/Eld/HosLog.cs) — uses `DutyStatus` enum (verify which values: OnDuty/OffDuty/Driving/SleeperBerth — these align with both US and EU but EU also has "AvailableTime")
- ELD providers in [Infrastructure.Integrations.Eld/](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/): Samsara, Motive, TtEld, Demo (Geotab/Omnitracs/PeopleNet enum values exist but no implementation)
- No HOS rule engine — violations appear hardcoded based on US thresholds

## Phasing recommendation

This is genuinely large. Suggest splitting into three sessions:

1. **Phase A — Rule engine abstraction** (~3 days). Refactor HOS calculation behind `IHosRuleSet` strategy; ship `FmcsaRuleSet` as default. No behavior change for US, but unblocks adding EU.
2. **Phase B — EU rule set** (~3 days). Implement `EuRuleSet561_2006` (driving/daily/weekly/biweekly). Make rule set selection driven by `Tenant.Settings.Region`.
3. **Phase C — Tachograph provider** (~5–10 days). Pick partner platform (Webfleet, Frotcom, Geotab Drive — Geotab has EU presence, simpler than direct VDO/Stoneridge). Implement `IEldProviderService` for one provider; add `Tachograph` enum value pointing to it.

Sequence: A → B → C. Phases A+B unblock manual / driver-self-reported HOS in EU (still useful) before tachograph integration lands.

## Backend (per phase)

### Phase A — Rule engine

#### Domain

- New interface `IHosRuleSet` in `Logistics.Domain/Services/Hos/`:
  - `string Code` (e.g., `"FMCSA_USA"`, `"EU_561_2006"`, `"AETR"`)
  - `IEnumerable<HosViolation> EvaluateAsync(IEnumerable<HosLog> logs, DateTime asOf)`
  - `HosLimits GetLimits()` returning daily/weekly/cycle limits for display
- Move FMCSA logic into `FmcsaRuleSet` implementation (`Infrastructure.Integrations.Eld/Rules/FmcsaRuleSet.cs`)
- Update `HosViolation.RuleCode` (string) so each violation knows which rule set it came from
- New value object `HosLimits` (record): `MaxDailyDriving`, `MaxDailyOnDuty`, `MaxWeeklyDriving`, `MinDailyRest`, `MinWeeklyRest`, etc. (in minutes)

#### Application

- Resolve rule set via `IHosRuleSetFactory.Create(Tenant tenant)` based on `tenant.Settings.Region`
- Update `Commands/Eld/SyncDriverHosStatusHandler` to use the factory instead of hardcoded thresholds

### Phase B — EU rule set

- New `Infrastructure.Integrations.Eld/Rules/EuRuleSet561_2006.cs`:
  - 4.5h continuous driving → 45-minute break
  - 9h daily driving (extendable to 10h twice per week)
  - 56h weekly driving max
  - 90h biweekly driving max
  - 11h daily rest (reducible to 9h three times per week)
  - 45h weekly rest (reducible to 24h every other week)
- New enum value `Region.Uk` (post-Brexit GB has its own slight variations) — or treat UK as EU and add an override later
- Add Display via `[Description("11-Hour Driving Limit")]` style — extend with EU equivalents like `[Description("4.5h Continuous Driving Limit")]`

### Phase C — Tachograph provider

- Add enum value `EldProviderType.Tachograph` (or be specific: `WebfleetTachograph`, `FrotcomTachograph`)
- New folder `src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/Webfleet/`:
  - `WebfleetEldProviderService : IEldProviderService` — auth (OAuth2), poll/webhook for tachograph data, map to `HosLog` (note: tachograph sample interval is much coarser than US ELD — 1 minute vs 1 second)
  - Models, mapper, options
- Register in [EldProviderFactory.cs](../../src/Infrastructure/Logistics.Infrastructure.Integrations.Eld/EldProviderFactory.cs)
- Webhook handler in `Application/Commands/Webhooks/Eld/Webfleet/`

### Persistence / migration

- New columns on `EldProviderConfiguration` only if needed for tachograph-specific tokens (likely fine as-is)
- New `HosViolation.RuleCode` column

## API

- `EldProviderConfigurationDto` — no shape change, just expand allowed `ProviderType` values
- New `GET /api/eld/rules/limits` returning `HosLimits` for the current tenant — UI uses this to render dashboards correctly
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- Translate hardcoded labels like "11-Hour Driving Limit" via the i18n system (plan #6)
- New helper in [LocalizationService](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts) — `formatHosDuration(minutes)` with locale-aware "h"/"min"

### TMS portal

- `pages/eld/components/hos-status-card/` — read limits from `/eld/rules/limits` instead of hardcoding
- `pages/eld/components/violation-list/` — render rule code badge ("FMCSA" / "EU 561/2006")
- `pages/eld/eld-providers/` — add tachograph providers to the dropdown when tenant region is EU
- New `pages/eld/eld-rules-info/` — read-only help page explaining the active rule set

### Admin portal

- No direct change beyond surfacing rule code in driver listings

## Mobile (driver app)

This is where it matters most — drivers see HOS counters all day.

- `model/HosLimits.kt` (KMP) — DTO mirroring API
- `viewmodel/HosViewModel.kt` — fetch limits from API instead of hardcoding 11h
- `ui/screens/HosScreen.kt` — render driving / on-duty counters dynamically against fetched limits, color-code based on percentage of limit (green/yellow/red)
- `ui/components/HosTimeline.kt` — already platform-agnostic; verify the timeline renders 4.5h break markers when EU rules active
- `composeResources/values/strings.xml` — string keys for all HOS labels (also covered by plan #6); add EU-specific strings (`hos_continuous_driving_limit`, `hos_45min_break`)
- `service/HosNotificationService.kt` — push warnings at correct thresholds per rule set (US: 30 min before 11h; EU: 30 min before 4.5h continuous)

## Tests

- `tests/Logistics.Application.Tests/Eld/` — exhaustive rule-set tests (driver scenarios → expected violations)
  - FMCSA: 11h driving, 14h window, 30-min break after 8h, 70/8 cycle
  - EU: 4.5h continuous, 9h daily, 11h rest, 56h/90h
- Property test: a sequence of `HosLog`s under each rule set never produces both "OK" and a "violation" of the same period — defensive

## Acceptance criteria

- [ ] US tenant: HOS dashboard, violation list, and driver mobile app behave identically to today (Phase A is non-breaking)
- [ ] EU tenant: violation appears when driver exceeds 4.5h continuous driving without a 45-min break
- [ ] EU tenant: dashboard shows EU limits (9h daily, 56h weekly), not 11h/70h
- [ ] Driver mobile app: warning push 30 min before 4.5h limit for EU drivers
- [ ] At least one tachograph provider connects, syncs HOS, and creates `HosLog` rows that the EU rule set evaluates correctly

## Update [.claude/feature-map.md](../feature-map.md)

Update the "ELD / HOS logs" row to mention rule sets; add new row:

| HOS rule sets | `IHosRuleSet`, `HosLimits` | - | `Infrastructure.Integrations.Eld/Rules/{Fmcsa,EuRuleSet561_2006}.cs` | `tms-portal/pages/eld/eld-rules-info/` |
