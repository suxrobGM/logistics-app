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

## Phase 0 — Spike: does PrimeNG 21 survive Angular 22 + CDK 22? _(throwaway branch, ~1 day)_

Gates everything.

1. Throwaway branch. Bump `@angular/*` → 22, add `@angular/cdk@22`, pin `typescript@~6.0.3`.
2. Add bun peer `overrides` in the **repo-root** `package.json` (the workspace root owning `bun.lock`) forcing `primeng`
   and `@primeuix/themes` to accept `@angular/core@22` **and `@angular/cdk@22`**.
3. `bun run build:all`, then smoke the **CDK-overlay-backed** surfaces — the likeliest cdk-skew breakage:
   `p-select`, `p-autocomplete`, `p-datepicker`, `p-dialog`, `p-confirmdialog`, `p-tooltip`, `p-popover`, `p-drawer`, `p-menu`.
4. Recreate the compat probe as a **committed** spec against v22-stable Signal Forms. Confirm the `BaseInput` `pattern`
   collision (6 components) and the `pTextarea` crash still hold, and probe the known `FormValueControl` sharp edges:
   angular/angular#65478 (value-as-`model()` recompute), #65576 (external model signal), #63625 (`min`/`max` + update).

**Exit criteria:** all five projects build; overlays work. If PrimeNG 21 breaks under Angular 22 / CDK 22, stop and
re-plan — the fallback is swapping the overlay component families to spartan _before_ the framework bump.

## Phase 1 — Angular 22 upgrade _(its own PR)_

1. `ng update @angular/core@22 @angular/cli@22` (+ `@angular/build`, `@angular/ssr`, `@angular/compiler-cli`,
   `ng-packagr`, `@ngrx/signals`, `angular-eslint`). `angular.json` sets `packageManager: bun`; if `ng update` misbehaves
   under bun, fall back to manual bumps + `ng update @angular/core --migrate-only --from=21 --to=22`.
2. **Revert the CD migration's `Eager` stamping** — keep the new `OnPush` default. Convert the 7 audit-list files to signals.
3. Pin `typescript` to `~6.0.3`. Bump CI Node to `^22.22.3 || ^24.15.0 || >=26.0.0`.
4. Set `paramsInheritanceStrategy: 'emptyOnly'` explicitly in each app's `provideRouter(..., withRouterConfig({...}))`.
5. Delete the deprecated `withFetch()` from `shared/src/lib/api/api.provider.ts`.
6. Verify/override peers for `angular-gridster2`, `ngx-mapbox-gl`, `angular-auth-oidc-client`, `@ngx-translate/*`, `@microsoft/signalr`.
7. Fix the `strictTemplates` tail from the optional-chaining semantics change.

**Gate:** `bun run build:all` + manual smoke of all four portals.

## Phase 2 — Safety net _(small, high leverage)_

1. Add `test` targets for `shared`, `tms-portal`, `admin-portal`, `customer-portal` (only `website` has one).
2. Commit the Phase 0 compat probe as a permanent spec in `shared`.
3. Playwright smoke per portal covering exactly what the sweep will touch: load form create/edit, customer form,
   dispatch board, one server-paged list, one dialog, one toast/confirm.
4. Add `.gitattributes` (`* text=auto eol=lf`) **before** any bulk sweep.
5. Add the test step to `.github/workflows/build.yml`.

## Phase 3 — Seam hardening, still on PrimeNG _(no visual change; each item independently shippable)_

1. **Rename shared `FormField` → `UiFormField`** (class only; selector `ui-form-field` unchanged). ~20 import sites.
2. **De-leak `ToastService`**: replace PrimeNG's `Confirmation` with an owned `ConfirmOptions`; move the `pi pi-*` icon
   and `p-button-danger` class inside the service. ~90 call sites unchanged.
3. **Delete the PrimeNG coupling in list infrastructure.** Replace `TableLazyLoadEvent` with an owned `ListLazyLoadEvent`,
   **collapse the two identical `base-list.store.ts` copies into one in `shared`**, and **delete the `BaseTable` abstract
   class** (2 of 82 users) in favor of the store.
4. **Introduce `<ui-data-table>`** wrapping `<p-table>`; migrate all 82 templates onto it. API sized to what is actually
   used: `[value]`, `[lazy]` + `(onLazyLoad)`, paginator/`[rows]`/`[totalRecords]`/`rowsPerPageOptions`, sort, `dataKey`,
   `scrollable`, selection, and projected `header`/`body`/`footer`/`emptymessage`/`caption` slots. Sweep the ~55 trivial
   and ~24 identical server-lazy tables in batches; hand-handle the ~6 hard ones — start with `trips-list`'s nested
   expansion table as the design forcing-function.
5. **Introduce `ui-*-field` wrappers implementing `FormValueControl` only** — `ui-text-field`, `ui-textarea-field`
   (native `<textarea>`, never `pTextarea`), `ui-select-field`, `ui-number-field`, `ui-date-field`, `ui-checkbox-field`,
   `ui-toggle-field`, `ui-multiselect-field`, `ui-autocomplete-field`. Internals drive PrimeNG via plain value/event
   bindings — **never** `formControlName`/`[formField]` on a PrimeNG element. Fold `ui-currency-field`/`ui-unit-field`
   into `ui-number-field` variants (delete the originals).
6. **Convert all 9 `ControlValueAccessor` implementations to `FormValueControl`**: `PhoneField`, `AddressForm`, and the 7
   tms-portal `search-*` / `address-autocomplete` components. Delete every `NG_VALUE_ACCESSOR` provider.

   Thanks to Angular 22's bridge these all drop into the **existing reactive forms** via `formControlName`. Adopt them
   across all ~52 forms here, with **zero Signal Forms work and zero shim code**.

7. `UiFormField` requires **no change**. Do not add dual-shape error handling.

## Phase 4 — Signal Forms migration _(~52 components)_

Templates already speak `ui-*-field`, so each form is a `.ts`-only change: `FormGroup` → `signal()` + `form()`.

- Use the `signal-forms-migration` skill — but **fix it first**: its "third-party components" section wrongly claims CVA
  "works with most third-party libraries" and recommends `[formField]` directly on PrimeNG.
- Order by risk: website (2) → admin-portal (6) → shared (4) → tms-portal (40).
- Model dynamic arrays as plain arrays inside the model signal (no `FormArray` class exists).
- Use the `{when: LogicFn}` config form of `disabled()`/`readonly()`/`hidden()`.
- Rewrite `ValidatedForm` for Signal Forms (its selector `form[formGroup]` never matches a Signal Form). Same UX
  contract: mark all touched, focus first invalid, `aria-live` count. **Delete the reactive-forms directive at the end of
  this phase**, along with the last `ReactiveFormsModule` import.
- **Do not use `compatForm` or `SignalFormControl`.** If a form resists conversion, fix the form.

**Exit criteria:** `git grep -E "ReactiveFormsModule|FormBuilder|new FormGroup|ControlValueAccessor|NG_VALUE_ACCESSOR"` is clean.

## Phase 5 — spartan/ui foundation + wrapper internals swap

1. `bun add @spartan-ng/brain @angular/cdk clsx tw-animate-css`; `bun add -d @spartan-ng/cli`.
   Generate helm components into `projects/shared/src/lib/spartan/` via `ng g @spartan-ng/cli:ui` (code-in-repo — ours).
2. Import `@spartan-ng/brain/hlm-tailwind-preset.css`; map spartan's shadcn CSS variables onto the token layer. **Promote
   `tms-portal/src/styles/variables.css` into `shared`** so all four apps use one token system. Reproduce the
   **load-bearing** parts of `primeng-preset.ts`: the entire dark `colorScheme` surface ramp, plus the visual signatures
   (uppercase datatable headers, tag sizing, gradient primary button).
3. Swap wrapper internals **one component type at a time**. Feature code untouched — the payoff of Phase 3.
4. Check each swap against the Phase 2 Playwright baseline.

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

| Artifact                                                             | Created    | Deleted                                 |
| -------------------------------------------------------------------- | ---------- | --------------------------------------- |
| `primeng` / `@primeuix/themes` peer overrides in root `package.json` | Phase 0    | Phase 7                                 |
| `p-table` internals inside `<ui-data-table>`                         | Phase 3    | Phase 7                                 |
| PrimeNG internals inside `ui-*-field` wrappers                       | Phase 3    | Phase 5                                 |
| `BaseTable` abstract class                                           | _(exists)_ | Phase 3                                 |
| Duplicate `base-list.store.ts` (admin copy)                          | _(exists)_ | Phase 3                                 |
| `ui-currency-field`, `ui-unit-field`                                 | _(exists)_ | Phase 3 (folded into `ui-number-field`) |
| 9 `ControlValueAccessor` implementations                             | _(exists)_ | Phase 3                                 |
| Reactive-forms `ValidatedForm` directive                             | _(exists)_ | Phase 4                                 |
| `ReactiveFormsModule` imports                                        | _(exists)_ | Phase 4                                 |
| `primeicons` + `pi pi-*` classes                                     | _(exists)_ | Phase 6                                 |
| `tailwindcss-primeui` + its utility classes                          | _(exists)_ | Phase 6                                 |
| `primeng-preset.ts`                                                  | _(exists)_ | Phase 7                                 |

---

## Standing risks

- **PrimeNG 21 under Angular 22 + CDK 22 is unproven.** Phase 0 exists solely to answer this. Prime suspect: CDK-backed
  overlays. If it fails, the order inverts — swap overlays to spartan _before_ the framework bump.
- **`paramsInheritanceStrategy` has no migration** and fails silently.
- **`FormValueControl` has open sharp edges**: angular/angular#65478 (value-as-`model()` recomputes on every keystroke),
  #65576 (cannot assign an external model signal), #63625 (`min`/`max` + value update throws). #67847 (CVA `writeValue`
  loopback) is **fixed**. Probe these in Phase 0 _before_ designing wrappers — the no-shim rule means there is no
  `compatForm` fallback if a wrapper design fails.
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
