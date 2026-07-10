# Angular 22 + Signal Forms + PrimeNG → spartan/ui — Migration Roadmap

> Status: rewritten 2026-07-09 against **Angular 22 stable**. Supersedes the 2026-07-09 draft, which was written
> against Angular 21's _experimental_ Signal Forms API and cited a compat probe that was never committed.
> Execute in phase order; each phase is independently shippable.

## Context

PrimeTek archived the PrimeNG repo on 2026-06-29. v22+ is commercial under the PrimeUI umbrella ($599/dev launch,
$799/dev from 2027, +$399/dev/yr for updates). Community (non-`-lts`) versions ≤21 remain MIT forever but are
unmaintained. We want off it because it will bit-rot against future Angular majors, and because Signal Forms cannot
bind `[formField]` to several PrimeNG controls (verified below).

**Target library: spartan/ui** — 1.0 stable (June 2026), MIT, Tailwind-4-native, CDK-based, shadcn-style code-in-repo.
Runner-up rejected: Taiga UI, ng-zorro, Angular Material. **OpenNG is not a fallback**: real, active org, but its first
cohort is the ngneat libraries; it has only blogged that it is _"considering"_ a PrimeNG fork. No fork repo, no npm package.

**The headline correction vs. the old draft:** the hard part of this migration is _not_ PrimeNG. Angular 22 changes the
default change-detection strategy to `OnPush` and silently flips the router's `paramsInheritanceStrategy`. This repo has
**371 components, none declaring `changeDetection`**. The framework upgrade is its own project and ships alone.

## Governing principle: no shims, no legacy residue

Every transitional artifact is recorded with the phase that **deletes** it (see Cleanup Ledger).

- **No dual-interface components.** `FormValueControl` only — never alongside `ControlValueAccessor`.
- **No `compatForm`, no `SignalFormControl`, no `@angular/forms/signals/compat`.** Every form converts fully.
- **No `ControlValueAccessor` survives.** All 9 implementations are converted, not wrapped.
- **No dual icon system.** `primeicons` and `@lucide/angular` do not coexist in the final state.
- **No `ChangeDetectionStrategy.Eager` markers.** The app is zoneless; take the new `OnPush` default.
- **No duplicated infrastructure.** The two identical `base-list.store.ts` copies collapse to one; `BaseTable` is deleted.
- **No leftover `tailwindcss-primeui` utility classes**, no `primeng-preset.ts`, no peer overrides.

`<ui-data-table>` and the `ui-*-field` components are **not shims** — they are the permanent public API. Only their
internals change (PrimeNG → spartan), once.

---

## Verified facts (2026-07-09)

### Angular 22

- Latest `22.0.6` (21.x line: `21.2.18`). Released 2026-06-03.
- **TypeScript `>=6.0.0 <6.1.0`.** TS 7.0 (the Go rewrite) is released but Angular's compiler does **not** support it;
  TS 7 can only serve as a side-channel `tsc` checker. Our `typescript: ^6.0.3` allows anything `<7.0.0` and **must be
  pinned to `~6.0.3`**.
- Node `^22.22.3 || ^24.15.0 || >=26.0.0`. RxJS `^6.5.3 || ^7.4.0`.

### Angular 22 breaking changes, scored against this repo

| Change                                                                                                                    | Repo impact                                          | Action                                                                                                                           |
| ------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **`OnPush` is the default CD strategy**; the migration stamps `ChangeDetectionStrategy.Eager` on every existing component | **371 components, 0 declare `changeDetection`**      | **Revert the stamping.** Already zoneless: 0 `\| async` pipes; 23 of 28 `.subscribe(` files write to signals. Audit the 7 below. |
| **Router `paramsInheritanceStrategy` → `'always'`** (was `'emptyOnly'`); **no migration provided**                        | Repo reads `snapshot.paramMap` widely                | Set `'emptyOnly'` explicitly via `withRouterConfig` in all 4 `app.config.ts`                                                     |
| Template optional chaining returns `undefined`, not `null`                                                                | `strictTemplates: true` → new type errors            | Expect a tail of template fixes                                                                                                  |
| HttpClient defaults to Fetch                                                                                              | `api.provider.ts:63` already calls `withFetch()`     | No-op; delete the deprecated `withFetch()`                                                                                       |
| Removed: `ComponentFactoryResolver`, `createNgModuleRef()`, `provideRoutes()`, `checkNoChanges()`                         | zero usages                                          | No-op                                                                                                                            |
| `provideAnimationsAsync()` deprecated                                                                                     | zero usages                                          | No-op                                                                                                                            |
| `reportProgress` deprecated                                                                                               | only in generated `api/generated/request-builder.ts` | Regenerated by `gen:api`; ignore                                                                                                 |

**OnPush audit list** (`.subscribe(` + plain-field assignment readable from a template): `customer-edit-dialog.ts`,
`change-role-dialog.ts`, `employee-edit-dialog.ts`, `employee-add.ts`, `timesheets-list.ts`,
`maps/address-autocomplete.ts`, `shared/.../form/address-form/address-form.ts`. Convert each to signals.
(`validated-form.ts`'s plain field is a DOM node, not template-read.)

### Signal Forms (v22, stable)

- Directive is `FormField`, selector `[formField]`.
- `FormValueControl<T>` requires **only** `value: ModelSignal<T>`. `FormCheckboxControl` requires `checked`.
  ~17 optional state inputs are auto-bound _only if the component declares them_: `disabled`, `readonly`, `hidden`,
  `invalid`, `errors`, `required`, `touched`, `dirty`, `pending`, `name`, `min`, `max`, `minLength`, `maxLength`,
  **`pattern: readonly RegExp[]`**.
- **The decisive fact.** angular.dev/guide/forms/signals/custom-controls states verbatim:

  > "Custom Signal Form Controls can be used with Signal, Reactive and Template-Driven Forms without any extra compatibility code."

  So a wrapper implementing `FormValueControl` **alone** works under both `formControlName` and `[formField]`. The old
  draft's dual `FormValueControl` + `ControlValueAccessor` design is unnecessary — it _would be_ the shim we refuse to write.

- v21→v22 changes: `disabled()`/`readonly()`/`hidden()` take `{when: LogicFn}`; `FieldState` optional props became
  required; `touched` model split into input + `touch()` output; `min`/`max` reject strings.
- **No `FormArray` class.** Dynamic arrays are plain arrays inside the model signal; `@for` over the iterable `FieldTree`.
- **No official Reactive→Signal Forms schematic.** Conversion is manual (see `.claude/skills/signal-forms-migration/`).

### `ui-form-field` needs no transitional code

Verified in `node_modules/@angular/forms/fesm2022/signals.mjs`: `[formField]` provides `NgControl` as `InteropNgControl`,
whose `errors` getter calls `signalErrorsToValidationErrors(field().errors())` — it emits the classic `ValidationErrors`
shape. Its `invalid`/`touched`/`dirty` getters are signal reads, so the existing `computed()`s track them. The existing
`contentChild(NgControl)` resolution works unchanged under **both** form systems. Do not add dual-shape error handling.

### PrimeNG ↔ Signal Forms interop — forensically verified against installed `node_modules`

(`@angular/core` 21.2.11, `@angular/forms` 21.2.11, `primeng` 21.1.6)

| Claim                                          | Verdict                                                                                                                                                                                                                                                              |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `pTextarea` + `[formField]` crashes at runtime | **VERIFIED.** `primeng-textarea.mjs:137-142` subscribes to `ngControl.valueChanges` in `onInit`. `[formField]` provides `NgControl` as `InteropNgControl` (`signals.mjs:1079`), which exposes no `valueChanges`/`statusChanges` — still true on v22 main.            |
| `pattern` collision breaks `strictTemplates`   | **VERIFIED.** `BaseInput.pattern: InputSignal<string \| null \| undefined>` vs Signal Forms `pattern?: InputSignal<readonly RegExp[]>`. Angular's compiler has a dedicated `expandBoundAttributesForField` type-check op that synthesizes the binding → hard TS2322. |
| Blast radius                                   | **6 components, not 4.** Every `BaseInput` subclass: `Select`, `InputNumber`, `DatePicker`, `AutoComplete`, **`InputMask`**, **`Password`**.                                                                                                                         |
| Safe set                                       | **VERIFIED.** `pInputText`, `p-checkbox`, `p-toggleswitch`, `p-multiselect`, `p-selectbutton` do not extend `BaseInput`. Across **all** of primeng's `fesm2022`, only `primeng-textarea.mjs` references `valueChanges`; nothing references `statusChanges`.          |

These only matter transitionally: **nothing ever binds `[formField]` to a PrimeNG element.** Wrappers drive PrimeNG
internally via plain value/event bindings, and `ui-textarea-field` uses a native `<textarea>` from day one.

### spartan/ui

| Fact                      | Value                                                                                                      |
| ------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `@spartan-ng/brain@1.1.0` | **MIT**; peer `@angular/core >=21.0.0 <23.0.0` → **Angular 22 supported**                                  |
| Other peers               | `@angular/cdk >=21 <23`, `@angular/forms`, `tailwindcss >=4`, `clsx`, `tw-animate-css`, `luxon` (optional) |
| `@spartan-ng/helm`        | **Does not exist on npm.** Helm code is generated into the repo via `ng g @spartan-ng/cli:ui`              |
| `@spartan-ng/cli@1.1.0`   | MIT, devDependency; Angular CLI supported (Nx optional)                                                    |
| New runtime deps          | `@angular/cdk@22.0.4` (MIT), `clsx`, `tw-animate-css`                                                      |

Fallback if spartan stalls: Angular Material, or build directly on `@angular/cdk` v22 — essentially what spartan is.

### PrimeNG licensing

`primeng@21.1.9`'s npm `license` field is `"SEE LICENSE IN LICENSE.md"`, not `MIT`. That file defines **two** licenses:
community versions are **MIT**; **`-lts`-suffixed versions are proprietary** (`20.5.1-lts`, `19.2.0-lts` are published).
**Never install an `-lts` dist-tag.** `primeng@22.0.0-rc.2` also exists.

`primeng@21.1.9` peers `@angular/core: ^21.0.7` **and `@angular/cdk: ^21.0.0`**. Angular 22 needs cdk 22; spartan needs
cdk `>=21 <23`. **Two peer overrides are required, not one.**

## Current footprint (measured 2026-07-09)

- **311 `.ts` files** import `primeng/*` (tms 229, admin 32, shared 20, website 15, customer 15). **48** distinct modules.
- Heaviest: button 206 · card 131 · tag 88 · **table 87** · tooltip 86 · progressspinner 74 · inputtext 53 · select 46 ·
  dialog 42 · textarea 36 · divider 33 · skeleton 32 · api 29 · menu 20 · datepicker 19 · inputnumber 19 · toast 16 ·
  confirmdialog 13 · checkbox 13 · autocomplete 12 · multiselect 10 · chart 10.
- **~52 distinct reactive-form components** (tms 40, admin 6, shared 4, website 2, customer 0), ~100 files with templates.
  _(The old draft's "97 files" was a `.ts`+`.html` touchpoint count, not a form count.)_
- **1 file already uses Signal Forms**: `admin-portal/.../tenants/tenant-edit/tenant-edit.ts`.
- **9 `ControlValueAccessor` implementations**: shared `phone-field`, `address-form`; tms `address-autocomplete` + 6 `search-*`.
- **105 `<p-table>` occurrences across 82 templates** — all raw. `shared/.../display/base-table/base-table.ts` is an
  _abstract logic class_ (`template: ""`, no `.html`), used by only **2 of 82**. There is no `<ui-base-table>` component.
- `base-list.store.ts` exists in **both** admin-portal and tms-portal, identical public contract; each only _type_-imports
  `TableLazyLoadEvent` from `primeng/table`.
- **`ToastService`** (`shared/src/lib/services/toast.service.ts`) already wraps _both_ `MessageService` and
  `ConfirmationService`; ~90 confirm sites funnel through it. Direct `primeng/api` imports survive only in the four
  `app.config.ts` files and `toast.service.ts`. It leaks PrimeNG at exactly three points: the `Confirmation` type,
  `icon: "pi pi-exclamation-triangle"`, `acceptButtonStyleClass: "p-button-danger"`.
- **All dialogs are declarative `[(visible)]`.** Zero `DialogService`/`DynamicDialog` usage.
- **Name collision:** `@logistics/shared`'s `ui-form-field` component class is named `FormField` — so is Angular's Signal
  Forms directive. ~20 files import the shared one.
- Wraps we can unwrap: `p-chart` → `chart.js` (already a dep), `p-editor` → `quill` (already a dep).

### Table feature surface (narrower than feared)

`[lazy]` 33 · `paginator` 47 · `[rows]` 50 · `rowsPerPageOptions` 37 · `sortField` 18 · `dataKey` 19 · `scrollable` ~20 ·
`selectionMode` 6 · templates `#header` 171 / `#body` 99 / `#footer` 34 / `#emptymessage` 27 / `#caption` 9 / `#expandedrow` 1.

**Zero** usages of `p-columnFilter`, `filterDelay`, cell/row editing, frozen / resizable / reorderable columns, virtual
scroll, row reorder, row grouping.

Buckets: **~55 trivial** client-side `[value]` tables · **~24 identical** server-lazy tables · **~6 hard** —
`trips-list` (row expansion containing a _nested_ `p-table`), `loads-table` (multi-select + checkboxes),
`trip-wizard-review`, `attach-load-dialog`, `trip-details` (selection), `trip-wizard-loads` (client global filter).

### The hidden removal tax

- **Icons.** `pi pi-*` appears ~580 times across 209 templates (~90 distinct icons). Lucide is registered but used in
  only ~8 files, with zero `<lucide-icon>` in templates. Remapping is a first-class workstream that **gates** removal.
- **`tailwindcss-primeui` utilities.** ~243 template usages. Those with no `@theme` fallback go unstyled the moment the
  plugin is dropped: `text-muted-color` ×35, `bg-surface-[0-9]` ×28, `border-surface` ×9, `text-surface-*` (TMS); ~29 in admin.
  `text-primary`/`bg-primary` survive in TMS via the `@theme` mapping but **not** in admin.
- **CSS coupling is small**: 6 files with `::ng-deep` / `.p-*` selectors.
- **Visual-break risk**: TMS **high** (real dark mode, hand-authored dark surface ramp) · admin **medium** · customer **low** · website **low**.

### There is no safety net today

**Zero `.spec.ts` files.** Only `website` defines a `test` target (`@angular/build:unit-test`, vitest-backed; vitest +
jsdom already installed). No Playwright/Cypress/e2e, no visual regression. CI runs `bun install --force` → `gen:api` →
`ng build` ×5, with **no lint step and no test step**. `bun run lint` is pre-existing red — `bun run build:all` is the
only real gate. `git config core.autocrlf=true` with **no `.gitattributes`** → bulk `sed -i` sweeps create phantom
EOL-only diffs.

---

## Phase 0 — Spike: does PrimeNG 21 survive Angular 22 + CDK 22? — ✅ **DONE (2026-07-09). Answer: YES.**

Executed on branch `feat/angular-22-upgrade` (started as `spike/angular-22-primeng-compat`).

**Result: `bun run build:all` exits 0.** All five projects — `shared` (ng-packagr partial compilation), `tms-portal`
(229 PrimeNG imports), `admin-portal` (already uses `@angular/forms/signals`), `customer-portal`, and `website`
(SSR, prerenders 8 routes) — build clean on Angular **22.0.6** + `@angular/cdk` **22.0.4** + `primeng` **21.1.6**.
`ng serve tms-portal` boots and serves the SPA shell (HTTP 200).

What we actually learned, versus what we feared:

| Expectation                          | Reality                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **CDK overlay skew is the top risk** | **Void.** PrimeNG 21 imports `@angular/cdk` in exactly **3** components — `listbox`, `orderlist`, `picklist` — and only `@angular/cdk/drag-drop`. It never uses CDK overlays (PrimeNG ships its own). **This repo imports none of those 3**, and no app code imports `@angular/cdk` at all. CDK 21→22 cannot affect us.                                                                                                                |
| **Peer overrides are needed**        | **Not needed.** `bun install` resolves cleanly with **no `overrides` block** and no peer errors — bun does not enforce peer ranges (npm would). Nothing was added to the root `package.json`.                                                                                                                                                                                                                                          |
| Node just works                      | **No.** Angular 22 requires Node `^22.22.3 \|\| ^24.15.0 \|\| >=26.0.0`. The dev machine was on 24.9.0 and the CLI refused to run. **CI has no `setup-node` step** — `.github/workflows/build.yml` uses `setup-bun` only, so this is a latent CI break.                                                                                                                                                                                |
| TypeScript range is comfortable      | The 6.x line stops at **6.0.3**; `7.0.2` is now npm `latest`. Pinned `~6.0.3`.                                                                                                                                                                                                                                                                                                                                                         |
| —                                    | **New: diagnostic NG1054.** A `model()` named `foo` implicitly creates a `fooChange` output; declaring an explicit one is now a hard error. **8 components hit this**, and every one had a real duplicate-emission bug (the template bound `[(ngModel)]` to the model _and_ the class emitted `fooChange` manually). Fixed by deleting the redundant explicit outputs. Public API unchanged — `model()` supplies the same output name. |

Remaining warnings only: pre-existing bundle-budget overages, a `quill`/CommonJS bailout, and one `NG8102`
(`session.decisions ?? []` where the left side is non-nullable).

**Still owed from this phase:** interactive smoke of overlay surfaces (`p-select`, `p-autocomplete`, `p-datepicker`,
`p-dialog`, `p-confirmdialog`, `p-tooltip`, `p-popover`, `p-drawer`, `p-menu`) in a real browser — a build cannot prove
runtime behavior. Folded into Phase 2's Playwright smoke.

## Phase 1 — Angular 22 upgrade _(its own PR)_

1. `ng update @angular/core@22 @angular/cli@22` (+ `@angular/build`, `@angular/ssr`, `@angular/compiler-cli`,
   `ng-packagr`, `@ngrx/signals`, `angular-eslint`). `angular.json` sets `packageManager: bun`; if `ng update` misbehaves
   under bun, fall back to manual bumps + `ng update @angular/core --migrate-only --from=21 --to=22`.
2. **Revert the CD migration's `Eager` stamping** — keep the new `OnPush` default. Convert the 7 audit-list files to signals.
3. Pin `typescript` to `~6.0.3` (the 6.x line ends at 6.0.3; `7.0.2` is npm `latest` and Angular rejects it).
4. **Add a `setup-node` step to `.github/workflows/build.yml`** pinning Node `24.15.0` — CI currently installs no Node
   at all, so the Angular CLI would refuse to run. Also bump local dev docs.
5. **Fix NG1054** — 8 components declared a `model()` plus a redundant explicit `<name>Change = output<T>()`.
   Delete the explicit output; `model()` already provides it. (Done: `date-range-picker`, `address-autocomplete`,
   6 × `search-*`. Each had a real duplicate-emission bug.)
6. Set `paramsInheritanceStrategy: 'emptyOnly'` explicitly in each app's `provideRouter(..., withRouterConfig({...}))`.
7. Delete the deprecated `withFetch()` from `shared/src/lib/api/api.provider.ts`.
8. `angular-gridster2` → `^22.0.0`. `ngx-mapbox-gl@14` already peers `^21 || ^22`; `angular-auth-oidc-client` peers
   `>=20`. **No peer `overrides` block is required** — bun does not enforce peer ranges.
9. Fix the `strictTemplates` tail from the optional-chaining semantics change (one `NG8102` so far).

**Gate:** `bun run build:all` (passes) + interactive smoke of all four portals.

## Phase 2 — Safety net _(small, high leverage)_ — **mostly DONE**

1. ✅ Added `test` targets (`@angular/build:unit-test`, vitest) + `tsconfig.spec.json` for `shared`, `tms-portal`,
   `admin-portal`, `customer-portal`. All five projects now have one.
2. ✅ Committed the compat probe: `projects/shared/src/lib/forms/signal-forms-compat-probe.spec.ts` — **9 tests, green.**
   It pins the five load-bearing claims (A–E) plus the three `FormValueControl` sharp edges, so CI screams if any drift.
   **This is the workspace's first executable test.**
3. ✅ `.gitattributes` added (`* text=auto eol=lf`). NB: a deliberate `git add --renormalize .` commit is still owed;
   it was intentionally NOT run here so it doesn't contaminate this diff.
4. ✅ CI: added `setup-node@24.15.0` and a `bun run ng test shared --watch=false` step to `.github/workflows/build.yml`.
5. ✅ **Interactive verification via the Playwright MCP** (no committed e2e suite — deliberately; a hand-written spec
   suite would need the API + identity server running, which CI does not have, and would be dead weight).
   Signed in as `owner@test.com` against the real backend (API `:7000`, identity `:7001` — note both serve **http**,
   not https). Eight routes (`/home`, `/customers`, `/loads`, `/loads/add`, `/customers/add`, `/reports/loads`,
   `/trucks`, `/employees`) render with **zero page errors**. The only console errors anywhere are Mapbox rejecting the
   literal unexpanded `${MAPBOX_TOKEN}` placeholder — environment config, unrelated to the upgrade.

   **Phase 0's outstanding overlay smoke is now discharged** — every CDK-suspect overlay works on Angular 22:

   | Overlay           | Result                                                              |
   | ----------------- | ------------------------------------------------------------------- |
   | `p-select`        | opens, 15 options                                                   |
   | `p-autocomplete`  | opens; selecting a customer drives the reactive form to `ng-valid`  |
   | `p-datepicker`    | panel opens; `ui-date-range-picker` presets write back and re-query |
   | `p-menu`          | row kebab opens with items                                          |
   | `p-confirmdialog` | opens via `ConfirmationService`; rejected cleanly, no data touched  |
   | `p-dialog`        | opens (invite-employee)                                             |
   | `p-tooltip`       | shows on hover                                                      |
   | `p-table`         | server-lazy rows + paginator                                        |

   Not exercised: `p-drawer`, `p-popover` (3–4 usages, mobile-only chrome).

**Verification protocol for later phases.** There is no automated visual-regression net, so every component-type swap in
Phases 5–6 must be re-driven through the Playwright MCP against this same route/overlay checklist before it lands.

## Phase 3 — Seam hardening, still on PrimeNG _(no visual change; each item independently shippable)_

> **Progress (branch `feat/angular-22-upgrade`):** items 1, 2, 3, 4, 6 ✅ — and item 5's wrappers all exist with specs.
>
> **Phase 3 is DONE.** 215 of the original 221 raw form controls go through wrappers; `formControlName` untouched.
> 10 wrappers exist (text / textarea / select / number / date / checkbox / toggle / multiselect / password /
> autocomplete), each with a spec. **90 tests across 11 spec files.**
>
> **6 controls stay raw, each blocked on a capability, not effort:** a projected `<ng-template #item>`
> (p-select x3, p-autocomplete x1), one `p-checkbox [value]` without `[binary]` (a checkbox-group member, not a
> boolean), and one `p-datepicker selectionMode="range"`. The 6 `search-*` components also use `<ng-template #item>`
> and so cannot adopt `ui-autocomplete-field` yet.
>
> **The `#item` trap.** PrimeNG resolves the item template in `ngAfterContentInit`, so an `@if`-guarded
> `<ng-template #item>` may never register — and an unconditional one makes p-select/p-autocomplete use custom
> rendering for EVERY option, blanking all 54 converted selects. Design the slot in Phase 5 against spartan, and
> verify it in a browser.
>
> ### Wrapper defaults must mirror PrimeNG's exactly. This bug class bit us four times.
>
> Each was invisible to `build:all` and to every passing spec. Each was found only by driving the browser, or by a
> reviewer reading PrimeNG's `.mjs`:
>
> | wrapper input                 | wrong default                     | symptom                                                                                                           |
> | ----------------------------- | --------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
> | `currentPageReportTemplate`   | `undefined`                       | paginator threw on **every** paginated table                                                                      |
> | `optionLabel` / `optionValue` | `""`                              | every select rendered its options as the literal `"empty"`; `optionValue=""` would store `undefined` on selection |
> | `iconDisplay`                 | `undefined` (PrimeNG: `"button"`) | every datepicker lost its calendar trigger                                                                        |
> | `appendTo`                    | `"body"` (PrimeNG: `undefined`)   | every overlay portalled out of its wrapper                                                                        |
>
> **Rule:** for every forwarded input, read the declared default in `node_modules/primeng/fesm2022/primeng-*.mjs`
> and copy it. Never invent `""`, `true`, or `"body"`. Boolean inputs need `transform: booleanAttribute` — templates
> write bare attributes (`showIcon`, `stripedRows`) which Angular passes as `""`.
>
> **Name collisions with `FormValueControl`.** It reserves `minLength`, `maxLength`, `min`, `max`, `pattern`,
> `required`, `readonly`, `disabled`, `invalid`, `errors`, `name`, `touched`, `dirty`, `pending`, `hidden` as
> validator-derived state inputs. `ui-autocomplete-field`'s "characters before searching" had to become
> `minQueryLength`, or Signal Forms would auto-bind over it. TS2416 catches these.
>
> One deliberate normalisation, not a bug: `fluid` defaults to `true` on select/multiselect/date, compensating for
> the `class="w-full"` the sweep drops. 42 selects did not previously set it. Worth a visual pass.
>
> **The wrapper pattern is proven, not assumed.** `projects/shared/src/lib/components/form/text-field/text-field.spec.ts`
> shows a `FormValueControl`-only component syncing both ways under `formControlName` AND `[formField]`, propagating
> `disabled`, and rendering errors through `ui-form-field` under both — with no transitional code. All 8 wrappers
> have an equivalent spec; 73 tests green.
>
> **Table sweep done.** All 82 templates use `<ui-data-table>`; 34 use `<th uiSortHeader="Field">`; 6 use
> `UiTableRowDirectives`. `primeng/table` is imported by exactly 3 files, all in
> `shared/src/lib/components/display/data-table/`. Phase 7 swaps those 3.
>
> Two traps recorded so the next person does not rediscover them:
>
> - A projected `<ng-template #header>` resolves DI against the CONSUMER, not the inner `p-table` — hence
>   `UiDataTable` re-provides `Table` from its own view via a lazy factory. Without it: `NG0201`.
> - `Paginator.currentPageReport` calls `currentPageReportTemplate.replace(...)` unguarded, so forwarding
>   `undefined` throws on **every paginated table**. Defaults must match PrimeNG's, not `undefined`.

1. ✅ **Renamed shared `FormField` → `UiFormField`** (class only; selector unchanged). 59 files.
2. ✅ **De-leaked `ToastService`**: owned `ConfirmOptions` with semantic `ConfirmIcon` / `ConfirmSeverity` tokens,
   mapped to PrimeNG inside the service. 29 calls across 27 files, verified lossless against `git show HEAD:`.
   That mapping table is where Phase 6 swaps primeicons for lucide, with zero call-site churn.
3. ✅ **Deleted the PrimeNG coupling in list infrastructure.** Owned `ListLazyLoadEvent`; the two `base-list.store.ts`
   copies collapsed into `@logistics/shared/stores` (kept the rxMethod one — its `switchMap` cancels in-flight
   requests); `BaseTable` deleted and its 2 subclasses moved to `createListStore`. Added
   `setFilters(f, { reload: false })` so a page can seed filters without a duplicate initial request.

4. ⬜ **Introduce `<ui-data-table>`** wrapping `<p-table>`; migrate all 82 templates onto it. API sized to what is actually
   used: `[value]`, `[lazy]` + `(onLazyLoad)`, paginator/`[rows]`/`[totalRecords]`/`rowsPerPageOptions`, sort, `dataKey`,
   `scrollable`, selection, and projected `header`/`body`/`footer`/`emptymessage`/`caption` slots. Sweep the ~55 trivial
   and ~24 identical server-lazy tables in batches; hand-handle the ~6 hard ones — start with `trips-list`'s nested
   expansion table as the design forcing-function.
5. 🟡 **Introduce `ui-*-field` wrappers implementing `FormValueControl` only** — `ui-text-field` ✅ (with spec) —
   (native `<textarea>`, never `pTextarea`), `ui-select-field`, `ui-number-field`, `ui-date-field`, `ui-checkbox-field`,
   `ui-toggle-field`, `ui-multiselect-field`, `ui-autocomplete-field`. Internals drive PrimeNG via plain value/event
   bindings — **never** `formControlName`/`[formField]` on a PrimeNG element. Fold `ui-currency-field`/`ui-unit-field`
   into `ui-number-field` variants (delete the originals).
6. **Convert all 9 `ControlValueAccessor` implementations to `FormValueControl`**: `PhoneField`, `AddressForm`, and the 7
   tms-portal `search-*` / `address-autocomplete` components. Delete every `NG_VALUE_ACCESSOR` provider.

   Thanks to Angular 22's bridge these all drop into the **existing reactive forms** via `formControlName`. Adopt them
   across all ~52 forms here, with **zero Signal Forms work and zero shim code**.

7. `UiFormField` requires **no change**. Do not add dual-shape error handling.

## Phase 4 — Signal Forms migration _(52 components)_ — ✅ COMPLETE

All 52 forms are Signal Forms. Exit criteria clean: 0 `ReactiveFormsModule` / `FormBuilder` /
`new FormGroup` / `new FormControl`, 0 `ControlValueAccessor` / `NG_VALUE_ACCESSOR` implementations,
0 compat shims, 0 `formControlName` / `[formGroup]` / `(ngSubmit)` / `[control]=` in templates.
39 `<form [formRoot]>`, 286 `[formField]` bindings, 21 `FormValueControl` controls.
`build:all` green, 98 tests / 13 spec files.

The `signal-forms-migration` skill was **rewritten against the installed 22.0.6** and is now pinned by
`signal-forms-v22-api-probe.spec.ts` (claims F–O). The old draft was wrong about `submit()`,
`form[formRoot]`, error kinds, status classes and `reset()`. **Read the skill before touching a form.**

### The one thing that broke, 46 times

**`[formField]` value types are invariant.** A control's `value` is a `ModelSignal<T>` (read+write), so
the model field type must equal the wrapper's `FormValueControl<T>` T exactly. Reactive `FormControl`s
were routinely `string | null`; `ui-text-field` is `FormValueControl<string>`. Convention now: optional
text fields hold `""`, coerced `dto.x ?? ""` inbound and `v.x || null` outbound. Every fix was checked
against `git show HEAD:<file>` so an empty field keeps sending the wire value it always did.

Knock-on: `pattern()` / `minLength()` now typecheck against `string`, so an **optional** field needs
`{when: ({valueOf}) => valueOf(p.x).length > 0}` — reactive `Validators.pattern` skipped empty values.

### Two bugs only the browser found

Both were invisible to `build:all` and to a fully green test suite — the same pattern as Phase 3's
four default-drift bugs.

1. **`invalid` is bound from form creation**, not from first interaction. Under the reactive bridge
   that state input was never driven, so wrappers' `[invalid]` / `[attr.aria-invalid]` were inert.
   After migration every required, empty, untouched field rendered as invalid on page load. Fixed by
   gating all 12 wrappers on `showInvalid = invalid() && (touched() || dirty())`.
2. **A conversion agent silently dropped a projected `<ng-template #item>`**, swapping a deliberately-raw
   `p-select` for `ui-select-field` and losing each ELD provider's description sub-line. The 6 raw
   controls from Phase 3 are raw _for a reason_. **When an agent converts a form, diff its template for
   dropped projected templates.**

### Shared seam

- `ui-form-field` resolves the projected `FORM_FIELD` token, reads `FieldState.errors()`, and renders
  `error.message` — so every validator carries one. The `NgControl` fallback, the `[control]` input and
  the reactive error flattening are gone. It keeps an explicit `[field]` input for the rare control that
  cannot carry `[formField]` (a raw PrimeNG component needing a projected template).
- `ValidatedForm` matches only `form[formRoot]`. It does **not** mark controls touched (`submit()`
  already does) and cannot query `.ng-invalid` (Signal Forms sets no status classes). It focuses via
  `errorSummary()[0].fieldTree().focusBoundControl()`, which is why every control implements `focus()`.
- `SearchTruck` gained a `[truckId]` seed input rather than widening `value` to `TruckDto | string | null`.
- `load-form.patch()` assigns field by field: `LoadFormValue` carries keys the model lacks, which
  `patchValue()` dropped but a `model.update()` spread would add to the field tree.

## Phase 5 — spartan/ui foundation + wrapper internals swap

**Step 1 is DONE.** Deps installed into the Angular workspace package (`bun add --cwd src/Client/Logistics.Angular`):
`@spartan-ng/brain@1.1.0` (MIT), `clsx`, `tw-animate-css`, `tailwind-merge`. `build:all` green, 98 tests green.
`@angular/cdk@22.0.4` was already present and satisfies brain's peer `>=21 <23`.

### Two facts the original plan got wrong

1. **`@spartan-ng/cli` is not installable here without dragging in Nx.** It depends on `nx`, `@nx/angular`,
   `@nx/devkit`, `@nx/js`, `@nx/workspace` and `@schematics/angular@21.2.14` — into a plain Angular CLI + bun
   workspace on Angular 22, with no `nx.json`. **User decision (2026-07-10): vendor Helm by hand, no CLI, no Nx.**
   Helm is code-in-repo either way; the CLI is only a copier.
2. **The Helm `.template` files are not directly usable.** A template's `classes(() => 'spartan-input …')` string is a
   _placeholder_. The generator builds a style map from the chosen `style-<theme>.css` (harvesting each
   `.spartan-x { @apply … }` rule) and **inlines those utilities into the component**, merging with `tailwind-merge`.
   Copy a template verbatim and you get a component referencing a class that nothing defines → unstyled.
   - `style-nova.css` (the default) holds **298** `.spartan-*` rules; **297** are the plain `{ @apply …; }` shape and
     extract with one regex. The lone exception is `spartan-drawer-content`.
   - The generator leaves three tokens in place: `spartan-invalid`, `spartan-menu-target`, `spartan-logical-sides`.
   - Get the sources without installing anything: `npm pack @spartan-ng/cli@1.1.0`, then read
     `package/src/generators/ui/libs/<primitive>/files/**` and `package/src/generators/ui/style-nova.css`.
   - Templates substitute exactly one variable, `<%- importAlias %>` (e.g. `import { classes } from '<%- importAlias %>/utils'`).
     Inside the `shared` library this must become a **relative** path — ng-packagr rejects tsconfig path aliases that
     resolve outside the entry point.

`utils` is the root primitive: `hlm()` (clsx + tailwind-merge), `classes()` (a `MutationObserver`-backed class manager),
and `provideSpartanHlm()` (sets `OVERLAY_DEFAULT_CONFIG.usePopover = false`, needed because Angular 21+ otherwise renders
CDK overlays above `position: fixed` elements).

### Remaining

2. Import `@spartan-ng/brain/hlm-tailwind-preset.css`; map spartan's shadcn CSS variables onto the token layer.
   The preset's `@theme inline` block expects these raw vars: `--background --foreground --card(-foreground)
--popover(-foreground) --primary(-foreground) --secondary(-foreground) --muted(-foreground) --accent(-foreground)
--destructive --border --input --ring --sidebar* --radius --font-sans --font-mono`.
   **Conflict to reconcile:** the preset declares `@custom-variant dark (&:is(.dark *))` while `tms-portal/styles.css`
   declares `@custom-variant dark (&:where(.dark-theme, .dark-theme *))`. Ours must be declared _after_ the preset import.
   **Promote `tms-portal/src/styles/variables.css` into `shared`** so all four apps use one token system (only TMS has
   one today; `projects/shared/src/styles/` is currently empty). Reproduce the **load-bearing** parts of
   `primeng-preset.ts`: the entire dark `colorScheme` surface ramp, plus the visual signatures (uppercase datatable
   headers, tag sizing, gradient primary button).
3. Swap wrapper internals **one component type at a time**. Feature code untouched — the payoff of Phase 3.
   Primitives the 12 `ui-*-field` wrappers need: `utils`, `input`, `textarea`, `select`, `checkbox`, `switch`, `label`,
   `field`, `input-group`, `date-picker` + `calendar` + `popover`, `autocomplete`/`combobox`, `button`.
   Note `HlmInput` is a **directive** on a native `<input>` (`[hlmInput]`), a drop-in for `pInputText`.
4. Check each swap against the Phase 2 Playwright baseline.

**Watch:** brain peers `luxon >=3.0.0` (only needed for `@spartan-ng/brain/date-time-luxon`); we have not installed it.

## Phase 6 — Non-form component sweep

- Cosmetic (~60% of usages): button (206), card (131), tag (88), tooltip (86), progressspinner (74), skeleton (32),
  divider (33), avatar/badge/chip/message/progressbar → spartan or plain Tailwind, behind `ui-*` where a pattern repeats.
- Behavioral: dialog (42), confirmdialog (13), toast (16), menu (20), popover/drawer/tabs/accordion/stepper/timeline.
  All dialogs are declarative `[(visible)]`; `ToastService` (hardened in Phase 3) absorbs toast/confirm with no call-site churn.
- Unwrap deps we already ship: `p-chart` (10) → `chart.js`; `p-editor` (1) → `quill`; galleria (1) → custom.
- Hand-roll: `inputmask` (1, inside `PhoneField`), stepper, timeline, galleria.
- **Icon workstream** (own milestone, gates Phase 7): remap ~90 distinct `pi pi-*` icons across ~209 templates to
  `@lucide/angular` behind the existing `ui-icon` component. Ends with a single icon system.
- **Replace all ~243 `tailwindcss-primeui` utility usages** with the shared `@theme` token layer.
- **No bulk `sed -i`** until Phase 2's `.gitattributes` has landed.

## Phase 7 — Table internals + PrimeNG removal

1. Swap `<ui-data-table>`'s internals from `p-table` to spartan/TanStack. Because Phase 3 centralized 82 templates behind
   it, this is **one component**, not 82. No column filters, editing, frozen columns, or virtual scroll are in use, so the
   required TanStack surface is small. Prototype against the loads list (server-side paging + sort) first.
2. Drop `primeng`, `primeicons`, `@primeuix/themes`, `tailwindcss-primeui`. Delete `primeng-preset.ts` and **both peer
   overrides** from the root `package.json`.
3. Docs sweep: `.claude/rules/frontend/angular-conventions.md` (lines 56–57 tell developers to _prefer_ `p-message`,
   `p-tag`, `p-table`, `p-dialog`), `src/Client/Logistics.Angular/CLAUDE.md` (Reactive Forms + `pInputText` examples;
   also missing admin-portal from its project table), `.claude/feature-map.md`, `.claude/skills/signal-forms-migration/SKILL.md`.
4. Final gate: `bun run build:all`, full smoke, and these must all return nothing outside docs/history:
   `git grep -i primeng`, `git grep "pi pi-"`, `git grep tailwindcss-primeui`, `git grep ControlValueAccessor`,
   `git grep ReactiveFormsModule`, `git grep ChangeDetectionStrategy.Eager`.

---

## Cleanup ledger — every transitional artifact and its deletion phase

| Artifact                                       | Created    | Deleted                                 |
| ---------------------------------------------- | ---------- | --------------------------------------- |
| ~~peer `overrides` in root `package.json`~~    | —          | **Never needed — bun ignores peers**    |
| `p-table` internals inside `<ui-data-table>`   | Phase 3    | Phase 7                                 |
| PrimeNG internals inside `ui-*-field` wrappers | Phase 3    | Phase 5                                 |
| `BaseTable` abstract class                     | _(exists)_ | Phase 3                                 |
| Duplicate `base-list.store.ts` (admin copy)    | _(exists)_ | Phase 3                                 |
| `ui-currency-field`, `ui-unit-field`           | _(exists)_ | Phase 3 (folded into `ui-number-field`) |
| 9 `ControlValueAccessor` implementations       | _(exists)_ | Phase 3                                 |
| Reactive-forms `ValidatedForm` directive       | _(exists)_ | Phase 4                                 |
| `ReactiveFormsModule` imports                  | _(exists)_ | Phase 4                                 |
| `primeicons` + `pi pi-*` classes               | _(exists)_ | Phase 6                                 |
| `tailwindcss-primeui` + its utility classes    | _(exists)_ | Phase 6                                 |
| `primeng-preset.ts`                            | _(exists)_ | Phase 7                                 |

---

## Standing risks

- ~~**PrimeNG 21 under Angular 22 + CDK 22 is unproven.**~~ **Resolved in Phase 0: it works.** All five projects build;
  PrimeNG never touches CDK overlays, so the feared version skew does not exist. This de-risks the entire
  "upgrade first, swap later" ordering.
- **Node version is a latent CI break.** Angular 22 requires Node `^22.22.3 || ^24.15.0 || >=26.0.0`, and
  `.github/workflows/build.yml` pins no Node at all (`setup-bun` only). Add `setup-node` before merging Phase 1.
- **`paramsInheritanceStrategy` has no migration** and fails silently.
- **`@ngrx/signals` has no Angular 22 release** (latest `21.1.1`, peer `^21.0.0`, no 22.x even in prerelease). It works
  today because bun does not enforce peers, but it is unmaintained against v22 — and it backs `base-list.store.ts`.
  Watch it; it is the most likely thing to break on Angular 23.
- ~~**`FormValueControl` has open sharp edges.**~~ **Probed on 22.0.6 — mostly non-issues.** Measured by
  `projects/shared/src/lib/forms/signal-forms-compat-probe.spec.ts`:
  - #65478 (value-as-`model()` recompute): **bounded**. `model()` uses `Object.is`, so a `computed()` over `value()`
    re-runs only on _distinct_ values, not on every raw `set()`. Not the per-keystroke cost the issue implies.
  - #65576 (external model signal): **does not reproduce**. No NG0318. But note the semantics: when a control is bound
    with both `[(value)]` and `[formField]`, **`[formField]` wins**. Wrappers must not expose an external two-way
    `value` binding alongside `[formField]`.
  - #63625 (`min`/`max` + value update): **works**. Validator-derived `min`/`max` push into the control's state inputs
    and re-validate correctly.
  - #67847 (CVA `writeValue` loopback) was already fixed upstream, and is moot — we implement no CVAs.
- **`ui-data-table` (Phase 3.4) is the single largest chunk of work.** It is also what makes Phase 7 cheap.
  `trips-list`'s nested-table row expansion is the hard case; design against it first.
- **The icon migration is a hidden project** (~90 icons, ~209 templates) and it _gates_ PrimeNG removal.
- **TMS dark mode is hand-authored** in `primeng-preset.ts`. Reproducing it in spartan tokens is the highest visual risk.
- **spartan is code-in-repo.** Upstream fixes don't flow automatically; track the upstream repo.
- Windows + `core.autocrlf=true` and no `.gitattributes`: broad `sed -i` sweeps create phantom EOL-only diffs — trust
  `git diff`, not `git status`. Phase 2 fixes this.

## Verification strategy (every phase)

- `bun run build:all` is the gate (shared/admin lint is pre-existing red — do not use lint as the gate).
- Keep the committed compat probe green in `shared` as wrappers evolve.
- After Phase 2, CI runs unit tests; every component-type swap in Phases 5–6 is checked against the Playwright baseline.
- Exercise real flows in the running portal (Playwright MCP available): load form create/edit, customer form, dispatch
  board, a server-paged list, one dialog, one toast/confirm — not just builds.
- Phase 7's `git grep` sweep is the definition of done.
