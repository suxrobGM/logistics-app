# Handoff: Multi-Language i18n (Angular + Mobile)

> **Priority:** MEDIUM (do BEFORE adding more pages — touches every component). **Effort:** L (~1 week scaffold + ongoing translation work).
>
> All Angular labels are hardcoded English. The mobile driver app already declares a `Language` enum (`en`/`ru`/`uz`) in [Settings.kt](../../src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/model/Settings.kt) but only one `strings.xml` exists. We need a real i18n pipeline before the codebase grows further.

## Sequencing

- **Position in overall order:** 4th
- **Depends on:** Nothing technical. Best done after plans #1–#3 to avoid translating strings that are about to be rewritten.
- **Unblocks:** Plan #4 ([tachograph HOS](handoff-eu-tachograph-hos.md)) — HOS labels need translation. Plan #7 ([driver licensing](handoff-driver-licensing-and-adr.md)) — license-class labels are region-specific and benefit from localization. Plan #10 ([address forms](handoff-region-aware-address-and-tenant-fields.md)) — country / state labels.
- **Why fourth:** Every plan after this one adds new UI strings. Doing i18n first means new code is born translated; doing it later means a second pass through every changed component.

## Why now

- Frontend has ~250 Angular component files; the longer we wait the more strings need extracting
- Mobile already promised `ru` and `uz` to users via the enum but doesn't deliver translations
- Date/number formatting is partially solved by [LocalizationService](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts) — translations are the missing piece

## Decision: which i18n library

- **Angular**: Use **`@ngx-translate/core`** (runtime translation, JSON files, supports lazy-loading per portal). Avoid Angular's built-in `@angular/localize` — requires a separate build per locale, hostile to multi-tenant runtime switching by `TenantSettings.Language`
- **Mobile (KMP)**: Use Compose Multiplatform's `stringResource` + `composeResources/values-{lang}/strings.xml` (Android-style resource folders work in CMP)

## Current state

- Angular `LocalizationService` already provides currency / unit / date format mapping — extend it, don't replace
- No `TenantSettings.Language` field yet — must add (or reuse `Region` for now and refine later)
- Mobile `Language` enum: `ENGLISH`, `RUSSIAN`, `UZBEK` — Russian and Uzbek implies the user already serves CIS markets; English is the only one with strings today
- A few hardcoded strings already use `i18n` attribute (look for `i18n="..."` in templates) — those are stale and need removal

## Backend

### Domain

- Add `TenantSettings.Language` (string ISO 639-1 code, default `"en"`) — ([TenantSettings.cs](../../src/Core/Logistics.Domain.Primitives/ValueObjects/TenantSettings.cs))
- Add `User.PreferredLanguage` (string?, nullable — falls back to tenant default if null)

### Application / Infrastructure

- No change to handlers — language is a presentation concern; only emails/PDFs need it server-side
- [Email templates (Fluid)](../../src/Infrastructure/Logistics.Infrastructure.Communications/Email/) — duplicate templates per language: `welcome.en.liquid`, `welcome.de.liquid`, etc. Email service picks template by user's language
- [InvoicePdfService](../../src/Infrastructure/Logistics.Infrastructure.Documents/Pdf/InvoicePdfService.cs) — extract its hardcoded English strings into a `Resources.{lang}.resx` and use `IStringLocalizer<InvoicePdfService>`

### Persistence / migration

- New tenant DB columns: `User.PreferredLanguage`, complex `TenantSettings.Language`
- Defaults: `'en'`

## API

- `TenantSettingsDto` add `Language`; `UserDto` add `PreferredLanguage`
- New `GET /api/i18n/translations/{lang}` — returns merged JSON of all keys for that language. The frontend caches this and rehydrates on language change. (Backend reads from a static folder, not the DB — translations are part of the build artifact.)
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- Install `@ngx-translate/core` and `@ngx-translate/http-loader`
- New `services/i18n.service.ts`:
  - Wraps `TranslateService`
  - On app start: reads `TenantSettings.Language` from auth payload, sets active language
  - Exposes `setLanguage(code)` that persists to `User.PreferredLanguage` via API
- Update `LocalizationService`:
  - Add `getLocale()` returning BCP47 code (e.g., `de-DE`) — combine `Language` + `Country`
  - Use that locale in `Intl.NumberFormat` / `Intl.DateTimeFormat` calls
- New `<ui-language-picker>` component
- Translation file structure:

  ```text
  projects/shared/src/lib/i18n/
    en.json
    de.json
    fr.json
    es.json
    ru.json
    uz.json
  ```

  Each portal merges shared + portal-specific:

  ```text
  projects/tms-portal/src/app/i18n/
    en.json
  ```

- Build script `bun run i18n:extract` — uses `ngx-translate-extract` to scan templates and merge new keys into JSON files

### Per portal

- Wire `TranslateModule.forRoot({ loader: ... })` in each portal's `app.config.ts`
- Replace hardcoded strings in templates with `{{ 'key.path' | translate }}`
- Pluralization: use `ngx-translate-messageformat-compiler` for ICU plural strings ("1 load" / "2 loads")

### Migration approach (avoid big bang)

1. Set up infrastructure + scaffolding (1–2 days)
2. Migrate **shared lib** first — every portal benefits immediately
3. Migrate TMS portal pages incrementally — newest pages first
4. Migrate customer portal + admin portal + website
5. Remove all hardcoded strings; CI step fails if a template contains a literal `>Some Text<` outside specific allowlist

### Translation memory

- Set up `bun run i18n:extract` to populate keys
- Use a translation service (Crowdin / Lokalise) — both have GitHub integration that opens PRs with translations; cheap for an MVP
- Keep en.json as source of truth in repo

## Mobile (driver app)

- Move all strings from inline composables into `composeResources/values/strings.xml`
- Add `composeResources/values-ru/strings.xml`, `values-uz/strings.xml`, and EU languages we need (`values-de`, `values-fr`, `values-pl`)
- Use `stringResource(Res.string.key)` everywhere
- `viewmodel/SettingsViewModel.kt` — change language → recompose with new locale (`Locale.setDefault` on Android side via `actual`)
- For date/number/currency formatting: extend the existing `expect/actual` `formatCurrency`/`formatDistance` helpers to be locale-aware
- HOS / load detail screens are highest priority (drivers see them constantly)

## Tests

- Snapshot tests of a few pivotal screens in each language to catch layout breaks (German is ~30% longer than English; check truncation)
- Lint rule: ban `>[A-Z][a-z].*<` in templates outside `<code>`/`<pre>` tags

## Acceptance criteria

- [ ] User with `PreferredLanguage = 'de'` logs in → entire UI in German
- [ ] User can change language from settings; saved to backend
- [ ] Email "Welcome to LogisticsX" arrives in German for German user
- [ ] Invoice PDF renders in tenant language
- [ ] Mobile app honors `Language` setting across all screens (currently only the enum exists)
- [ ] Date/currency formatting follows locale (e.g., `1.234,56 €` for de-DE, `$1,234.56` for en-US)

## Update [.claude/feature-map.md](../feature-map.md)

Add row under "Settings & integrations":

| i18n / translations | `User.PreferredLanguage`, `TenantSettings.Language` | `services/i18n.service.ts` | `Email/templates/*.{lang}.liquid`, `Resources/*.{lang}.resx` | `<ui-language-picker>`, all portals |
