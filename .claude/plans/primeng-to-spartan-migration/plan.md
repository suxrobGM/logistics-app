# PrimeNG → spartan/ui Migration Roadmap

> Status: planned 2026-07-09. Execute in phase order; each phase is independently shippable.
> Companion file: [signal-forms-compat-probe.spec.ts](signal-forms-compat-probe.spec.ts) — the TestBed probe that produced the verified findings below. To re-run: drop it into `projects/website/src/app/`, temporarily add `"angularCompilerOptions": {"strictTemplates": false}` to `projects/website/tsconfig.spec.json`, run `bun ng test website`, then remove both.

## Context

PrimeTek archived the PrimeNG repo on 2026-06-29; v22+ moves to a commercial closed-source license (PrimeUI). PrimeNG ≤21 stays MIT forever but is unmaintained. This blocks nothing mechanically — but we want off it because:

1. It will bit-rot against future Angular majors (already no official Angular 22 peer support).
2. **Verified**: Signal Forms `[formField]` on PrimeNG 21 is partially broken (details below), so the Angular 22 + Signal Forms migration cannot bind `[formField]` directly to PrimeNG components.

**Decisions made (with user):** target library is **spartan/ui** (1.0 stable, Tailwind-native, shadcn-style code-in-repo — fits our Tailwind 4 token system and already-installed `@lucide/angular`; no future relicense risk). Full roadmap through complete PrimeNG removal. Runner-up considered and rejected: Taiga UI (team explicitly waiting for Signal Forms API to stabilize; competes with our Tailwind theming), ng-zorro (Ant look, less/CSS theming), Angular Material (component gaps, Material look).

## Verified findings (empirical, 2026-07-09, primeng 21.1.6 + @angular/forms 21.2.11)

Signal Forms + PrimeNG interop, from the probe (10 tests):

| Component                                                                                                 | Verdict                                                                                                                                                                                                                                                    |
| --------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `pTextarea`                                                                                               | **Runtime crash**: `ngControl.valueChanges` is undefined — Signal Forms' `InteropNgControl` implements no `valueChanges`; PrimeNG subscribes in `onInit` (`primeng-textarea.mjs` ~line 139). Repo archived → will never be fixed. ~36 files use it.        |
| `p-select`, `p-inputnumber`, `p-datepicker`, `p-autocomplete`                                             | **Compile failure under `strictTemplates: true`** (our setting, root `tsconfig.json`): Signal Forms auto-binds field state `pattern: readonly RegExp[]` onto PrimeNG `BaseInput`'s `pattern: string` input → TS2322. Runtime works if type-check bypassed. |
| `pInputText`, `p-checkbox`, `p-toggleswitch`, `p-multiselect`, `p-selectbutton`, `p-datepicker` (runtime) | Work in both directions.                                                                                                                                                                                                                                   |

**Consequence:** forms must migrate to Signal Forms _through a wrapper layer that implements `FormValueControl`_, never by putting `[formField]` on a PrimeNG component directly.

**Caveat:** these findings were measured against the **v21 experimental** Signal Forms API. Signal Forms is **stable in Angular 22** (released 2026-06-03). The PrimeNG-side root causes (the `ngControl.valueChanges` subscription, the `pattern: string` input) are frozen in the archived repo and won't change — but Angular's interop shim (`InteropNgControl`, state auto-binding) may have evolved in v22. **Re-run the probe immediately after Phase 0** and update this table before building Phase 1.

## Current footprint (measured)

- **311 files** import `primeng/*` (tms-portal 229, admin-portal 32, shared 20, website 15, customer-portal 15), 49 distinct modules.
- Heaviest: button 206 · card 131 · tag 88 · **table 87** · tooltip 86 · progressspinner 74 · inputtext 53 · select 46 · dialog 42 · textarea 36 · divider 33 · skeleton 32 · api (Confirmation/MessageService) 29 · menu 20 · datepicker 19 · inputnumber 19 · toast 16 · confirmdialog 13 · checkbox 13 · autocomplete 12 · multiselect 10 · chart 10.
- **Reactive forms: 97 files** (tms 77, admin 12, website 4, shared 4, customer 0).
- Dominant form pattern: raw PrimeNG control + `formControlName` projected inside `ui-form-field` chrome (58 templates). Self-contained wrappers (`ui-currency-field`, `ui-unit-field`, `ui-phone-field`, `ui-search-field`, `ui-address-form`) hide PrimeNG in only ~34 files.
- Existing shared CVAs: `PhoneField` (`projects/shared/src/lib/components/form/phone-field/phone-field.ts`), `AddressForm` (`.../address-form/address-form.ts`). Feature-level CVAs exist too (e.g. `tms-portal/.../truck-form/truck-vin-field.ts`).
- `ValidatedForm` directive (`.../form/validated-form/`) is reactive-forms-specific (marks touched, scrolls to `.ng-invalid`).
- Deepest coupling: `p-table` (87 files) + `base-list.store.ts` (admin-portal `shared/stores/`) which imports `primeng/table` types.
- Wraps we can unwrap: `p-chart` → `chart.js` (already a direct dep), `p-editor` → `quill` (already a direct dep).

---

## Phase 0 — Angular 22 upgrade with pinned PrimeNG 21

PrimeNG 21 is MIT forever; do not let it block the framework upgrade.

1. `ng update @angular/core@22 @angular/cli@22` (+ `@angular/ssr`, `ng-packagr`, `angular-eslint`, `@ngrx/signals` as needed).
2. PrimeNG 21 declares no Angular 22 peer support ([primeng#19608](https://github.com/primefaces/primeng/issues/19608)) — add a bun `overrides` entry in `src/Client/Logistics.Angular/package.json` to force-accept the peer range for `primeng` and `@primeuix/themes`.
3. Gate: `bun run build:all` (lint is pre-existing red — builds are the gate), then manual smoke of all four portals (`start:tms` etc.).
4. Check `angular-gridster2` / `ngx-mapbox-gl` / `angular-auth-oidc-client` Angular 22 releases at the same time.
5. **Re-run the compat probe** (instructions in the header of this file) against the now-stable v22 Signal Forms API and update the Verified findings table — this determines exactly which Phase 1 wrappers can lean on CVA interop vs. plain bindings.

Estimated blast radius: package.json + mechanical `ng update` migrations only.

## Phase 1 — FormValueControl field-wrapper layer (the keystone)

Create self-contained field components in `projects/shared/src/lib/components/form/` that **implement both `FormValueControl` (Signal Forms) and `ControlValueAccessor` (legacy)** so they work in old and new forms during the transition. Internally they drive the widget via plain value/event bindings — **never** `formControlName`/`[formField]` on the PrimeNG element (avoids both the `InteropNgControl` crash and the `pattern` type collision).

New components (name → initially wraps → replaces raw usage of):

- `ui-text-field` → native `<input pInputText>` → pInputText (53)
- `ui-textarea-field` → **native `<textarea>` + Tailwind** (do NOT keep `pTextarea` — it's the crasher; visual parity via `variables.css` tokens) → textarea (36)
- `ui-select-field` → `p-select` → select (46)
- `ui-number-field` → `p-inputnumber` → inputnumber (19); fold in existing `ui-currency-field`/`ui-unit-field` variants
- `ui-date-field` → `p-datepicker` → datepicker (19)
- `ui-checkbox-field` / `ui-toggle-field` → `p-checkbox` / `p-toggleswitch` (13 + 6)
- `ui-multiselect-field` → `p-multiselect` (10)
- `ui-autocomplete-field` → `p-autocomplete` (12)

Implementation notes:

- `FormValueControl` needs only `value = model<T>()` plus optional state inputs (`disabled`, `required`, `errors`, `touched` …) that Signal Forms auto-wires. Add the CVA methods alongside; delete them in Phase 6.
- Convert existing `PhoneField` and `AddressForm` CVAs to the same dual pattern.
- Extend `ui-form-field` chrome: it currently auto-resolves `NgControl` from projected content (`contentChild(NgControl)`). Add detection of the Signal Forms binding (inject/`contentChild` the `FORM_FIELD` token from `@angular/forms/signals`) so error/touched rendering works for both form systems.
- Add a Signal Forms counterpart to `ValidatedForm` (on invalid submit: mark all touched via the field tree, scroll/focus first invalid, aria-live count — same UX contract).
- Update `.claude/skills/signal-forms-migration/SKILL.md` third-party section with the verified findings (currently says CVA fallback "works with most third-party libraries" — for PrimeNG it often doesn't).

Verification: adapt the probe spec to the new `ui-*-field` components (bind `[formField]` to wrappers, assert two-way sync) and keep it as a permanent spec in `shared`.

## Phase 2 — Signal Forms migration (97 form files)

Per-form conversion using the existing `signal-forms-migration` skill, replacing projected raw PrimeNG controls with Phase 1 `ui-*-field` wrappers in the same pass (one touch per template).

- Order by risk: website (4) → admin-portal (12) → tms-portal (77). Customer-portal has none.
- Representative heavy templates: `domain-forms/customer-form/`, `domain-forms/load-form/`, `trucks/components/truck-form/`, admin `tenant-form`.
- Keep `compatForm` (`@angular/forms/signals/compat`) in the back pocket for the gnarliest forms; don't reach for it by default.
- Gate per batch: `bun run build:all` + exercise the form in the running portal (Playwright MCP or manual).

## Phase 3 — spartan/ui foundation + wrapper internals swap

1. Install spartan CLI; copy brain/helm components into `projects/shared/src/lib/spartan/` (code-in-repo — treat as ours, but track upstream updates).
2. Theme integration: map spartan's Tailwind CSS variables onto the existing token layer (`projects/tms-portal/src/styles/variables.css`, `bg-elevated`/`bg-subtle`/`border-default`/`text-muted`); replicate the customizations currently in `projects/tms-portal/src/app/core/theme/primeng-preset.ts`. Icons: lucide already installed.
3. Swap Phase 1 wrapper internals from PrimeNG to spartan **one component type at a time** (feature code untouched — this is the payoff of the wrapper layer). While copied in-repo, convert spartan's form components to native `FormValueControl` and drop the CVA shim inside wrappers.
4. Visual regression: compare portals before/after per component type.

## Phase 4 — Non-form component sweep (bulk, mostly mechanical)

- Cosmetics, roughly 60% of all usages: button (206), card (131), tag (88), tooltip (86), progressspinner (74), skeleton (32), divider (33), avatar/badge/chip/message/progressbar → spartan equivalents or plain Tailwind. Where a pattern repeats, add a `ui-` shared component first, then sweep.
  - ⚠ Windows + `core.autocrlf=true`: broad `sed -i` sweeps create phantom EOL-only diffs — trust `git diff`, not `git status` (see memory: sed-crlf-gotcha).
- Behavioral: dialog (42) + confirmdialog (13) + toast (16) + menu (20) + popover/drawer/tabs/accordion/stepper/timeline/fieldset → spartan equivalents.
  - `primeng/api` `ConfirmationService`/`MessageService` (29 files): build shared `ConfirmService`/`ToastService` with near-identical call signatures backed by spartan dialog/sonner, so call sites change imports only.
- `p-chart` (10) → thin shared wrapper over `chart.js` directly; `p-editor` (1) → quill directly; galleria (1) → spartan carousel or minimal custom.
- Update `.claude/rules/frontend/angular-conventions.md` (currently says "Prefer PrimeNG … `p-message`, `p-tag`, `p-table`, `p-dialog`") and `src/Client/Logistics.Angular/CLAUDE.md` form examples as each equivalent lands.

## Phase 5 — Data table (hardest, last)

- Build `ui-data-table` on spartan's TanStack-based data-table. TanStack supports manual/server-side pagination+sorting+filtering — design the adapter around the existing `base-list.store.ts` contract (admin-portal `shared/stores/base-list.store.ts`, currently importing `primeng/table` types; tms-portal likely has a sibling — locate before starting).
- Prototype FIRST with one gnarly list (loads list with filters + lazy load) before committing to the sweep of ~87 files.
- Migrate list pages incrementally; `p-table` may legitimately be the last PrimeNG import standing for a while.

## Phase 6 — Removal

1. Drop deps: `primeng`, `primeicons`, `@primeuix/themes`, `tailwindcss-primeui`. Delete `primeng-preset.ts` and primeicons CSS imports.
2. Delete the CVA halves of the dual wrappers; delete `ReactiveFormsModule` remnants and the legacy `ValidatedForm` if all forms migrated.
3. Sweep docs: root `CLAUDE.md`, workspace `CLAUDE.md`, `.claude/rules/frontend/angular-conventions.md`, `.claude/feature-map.md`.
4. Final gate: `bun run build:all`, full portal smoke, `git grep -i primeng` returns nothing outside docs/history.

## Standing risks / watch items

- **Signal Forms is stable as of Angular 22** (2026-06-03), so the `FormValueControl` contract is safe to build on — but our compat probe ran against the v21 experimental API; re-verify after Phase 0 (see caveat under Verified findings). While still on Angular 21 (pre-Phase 0), the workspace has only the experimental API. The wrapper layer confines any remaining churn to `projects/shared/src/lib/components/form/`.
- **OpenNG fork** (openng.org): if it matures with real maintainers, it extends PrimeNG-21 runway (security/Angular-compat fixes) and relaxes urgency on Phases 4–5. Costs nothing to watch. Do not bet on it.
- spartan gaps to expect: no inputmask (1 use — `PhoneField`; find a mask lib or hand-roll), stepper/timeline/galleria may need custom builds.

## Verification strategy (every phase)

- `bun run build:all` is the gate (shared/admin lint is pre-existing red — do not use lint as the gate).
- Keep the adapted compat probe green in `shared` as wrappers evolve.
- Exercise real flows in the running portal (Playwright MCP available) — load form create/edit, customer form, dispatch board — not just builds.
