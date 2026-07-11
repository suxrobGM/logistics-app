# PrimeNG → spartan/ui — the finished record

> **Status: COMPLETE (2026-07-11).** PrimeNG is gone: 0 dependencies, 0 imports, 0 `<p-*>` markup,
> 0 `.p-*` selectors, no theme preset. `tools/gates/phase7.sh` runs in CI and fails the build if any
> of it comes back.
>
> This file used to be a roadmap. It is now the record. It is written for the person who has to do
> the next migration of this kind — so it leads with what we got **wrong**, and with the bugs we
> **found**, not with what we shipped.

---

## Outcome

|                                  |                                                                                                                 |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| PrimeNG surface (burndown total) | **3,690 → 0**                                                                                                   |
| Sites touched                    | ~2,300 (498 buttons · 940 icons · 608 cosmetics · 124 tooltips · 46 dialogs · 90 tables · 55 raw form controls) |
| Forms                            | 52 components → 100% Signal Forms. Zero `ReactiveFormsModule`, zero `ControlValueAccessor`                      |
| Tests                            | 0 → **244** (26 files). The repo had **no test suite at all** when this started                                 |
| Commits                          | 60, on `feat/angular-22-upgrade`                                                                                |
| Bundles                          | every app **below** its pre-migration size (see below)                                                          |
| Bugs found in existing code      | **9** (see the ledger) — several user-visible and long-lived                                                    |

### Bundles (measured, not estimated)

Measured by building the pre-migration commit in a worktree, because the planning numbers were
wrong. The website's "0.32 MB" was really **1.18 MB**; had we trusted the plan we would have
"discovered" a phantom 3× regression and gone hunting for it.

| app      | pre-migration (measured) | final       | delta  |
| -------- | ------------------------ | ----------- | ------ |
| tms      | 3.99 MB                  | **3.60 MB** | −9.8%  |
| customer | 1.58 MB                  | **1.02 MB** | −35.4% |
| admin    | 1.82 MB                  | **1.22 MB** | −33.0% |
| website  | 1.18 MB (_not_ 0.32)     | **1.01 MB** | −14.4% |

---

## The corrected facts

The old plan stated these confidently. A future reader would have believed them. **They were false.**
This section exists because the single most expensive habit in this migration was trusting the brief
over the source.

| The plan claimed                                              | The truth                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| "`git grep NG_VALUE_ACCESSOR` is currently empty by design"   | **It was not empty.** The four vendored Helm form primitives (`hlmInput`, `hlmTextarea`, `hlmCheckbox`, `hlmSelect`) each _provided_ `NG_VALUE_ACCESSOR` and implemented `ControlValueAccessor` — the exact legacy interface the migration existed to delete, re-entering through the new library. `tools/normalize-helm.mjs` now strips CVAs from every vendored primitive on the way in. It is 0 today because a codemod _makes_ it 0, not by design. |
| "6 raw form controls remain, blocked on a wrapper capability" | **55, across ~30 files** — roughly 10× the estimate — plus `pInputText` / `pTextarea` / `pInputTextarea` (a _third_ spelling nobody had named) in 22 more files. The count came from grepping one spelling of one thing.                                                                                                                                                                                                                                |
| "`p-card` is 100% content projection, zero templates"         | **83 template slots.** `p-card` accepted `#header` / `#title` / `#subtitle` / `#content` / `#footer` refs, and the repo used them heavily. A content-projection-only `ui-card` would have silently dropped every one.                                                                                                                                                                                                                                   |
| "All four portals share a PrimeNG theme"                      | **They do not.** tms-portal ran `definePreset(Nora, …)`; admin, customer and website ran **Aura**. The two presets disagree semantically — Aura's `.p-tag-success` is a soft tint (green-100 bg, green-700 ink), Nora's is a **solid fill** (green-600 bg, white ink). "Port the theme" was really "port two themes." The website also had `darkModeSelector: false` — no dark mode at all.                                                             |
| `[draggable]` / `[resizable]` on dialogs "must be kept"       | **Inverted.** They are the _opt-outs_; `primeng-dialog.mjs` defaults both to false. Keeping them would have made 13 dialogs draggable that never were.                                                                                                                                                                                                                                                                                                  |
| `p-select` needs one projected slot                           | **Two.** `eld-driver-mappings` projects `#item` _and_ `#selectedItem`. A one-slot wrapper flattens every trigger, silently.                                                                                                                                                                                                                                                                                                                             |
| "The chart.js saving is ~180 KB"                              | **Off by 13×.** The tree-shaken saving is ~14 KB min.                                                                                                                                                                                                                                                                                                                                                                                                   |
| "`selectButton` is 2 sites"                                   | 3. (A third spelling. Again.)                                                                                                                                                                                                                                                                                                                                                                                                                           |

**Roughly 15 "verified facts" in the brief turned out to be wrong, and the implementing agent was
right every single time.** The pattern is always the same: a fact derived from a grep of one spelling,
or from a library's _docs_, rather than from its compiled metadata and its **defaults**.

---

## The bug ledger — 9 bugs this migration FOUND

None of these were caused by the migration. All of them were live, most were long-lived, and **every
one was green on `build:all`, green on the test suite, and green on lint.** This is the most valuable
section in the document.

1. **NG0201 emptied `/loads` — the core page of a trucking TMS.**
   `<ui-data-table>` re-provided PrimeNG's `Table` so projected `#header`/`#body` templates could
   resolve it, but not `TableService`. `TableCheckbox`, `TableHeaderCheckbox` and `SelectableRow`
   inject **both**; `TableService` is `@Injectable()` with no `providedIn`, so it lives only in
   `Table`'s node providers and is invisible to the consumer's injector. Every projected checkbox
   threw NG0201 and took the whole table render down. The page rendered a summary card reading
   _"Total Loads: 195 / $22,233.00"_ directly above an **empty** grey table reading _"0 of 0"_.
   Also hit payroll-invoices-list, attach-load-dialog, trip-wizard-review, trip-details.

2. **Every client-paginated table rendered zero rows.**
   `first` and `totalRecords` were `input<number|undefined>(undefined)` and forwarded
   unconditionally, so Angular wrote `undefined` over `p-table`'s own `_first = 0` /
   `_totalRecords = 0` defaults. `slice(undefined, NaN)` → `[]`. 14 templates, incl. all 6 report
   pages. **Never forward `undefined` for an input whose library default is not `undefined`.**

3. **A live XSS.** `route-badge` set `[escape]="false"` on a `pTooltip` — PrimeNG renders that via
   `innerHTML` — and fed it a hand-built HTML string with **tenant address fields interpolated into
   it**. Forbidden by our own `angular-security.md`. Killed by converting the site to a `TemplateRef`
   tooltip, so the bindings escape.

4. **Escape discarded a half-filled form (data loss).**
   `ui-dialog` bound `(document:keydown.escape)`. A `ui-select-field` opened _inside_ a dialog is its
   own CDK overlay, so **one** Escape — meant for the dropdown — also reached the dialog and
   discarded the form. Same for nested confirm dialogs. It was hidden under PrimeNG because
   `primeng-select.mjs:1512` called `stopPropagation()` on Escape, so `p-dialog`'s document listener
   never saw it; Helm/brain let it bubble. Fixed with an `isTopmostOverlay` guard.
   **A document-level Escape handler fires for nested overlays too.**

5. **`selectButton` deselect-to-null.** `allowEmpty` defaults **true**, so clicking the _active_
   segment wrote `null` into three non-nullable targets (`signal<FilterType>`,
   `setLayer(MapLayerType)`, `onModeChange(PayrollMode)`) — the map style persisted as null and the
   payroll form body vanished. Spartan's `BrnToggleGroup.nullable` _also_ defaults true, so it had to
   be bound `false` explicitly or the bug would have been faithfully reproduced one layer down.

6. **`fileUpload` POSTed to `/undefined`** on every selection. Both sites set `[auto]="true"` with no
   `url`, so PrimeNG's uploader fired `http.request('post', undefined)` and latched "uploading"
   forever while the app uploaded separately in `(onSelect)`.

7. **`upcoming-service` never sorted — broken since it was written.** Its client-side table used
   PascalCase sort fields (`"TruckNumber"`) while the rows hold `truckNumber`. Client tables resolve
   the field off the **row object**, so every value was `undefined`: the header flipped to
   `aria-sort="ascending"` and the rows never moved. `git show` proves the original `p-table` had the
   identical `pSortableColumn="TruckNumber"`. The port reproduced it faithfully, then fixed it.

8. **Every sort arrow in all four portals rendered blank.** The new paginator asked for
   `lucideChevronsUpDown` / `Left` / `Right`; nothing registered them. See vacuous gate #2 below —
   the icon gate whose entire job is to catch a blank icon could not see the file that caused it.

9. **Keyboard-only tooltips would never have opened.** `BrnTooltip` binds its triggers and
   `aria-describedby` to its **own host**, and 82 of the 124 hosts are `<ui-button>` — a _wrapper_
   around the real `<button>`. **`focus` and `blur` do not bubble**, so brain's listener on the
   wrapper would never fire. Invisible to every gate, and it fails exactly the users a tooltip exists
   to serve. `uiTooltip` uses `focusin`/`focusout` (which do bubble) and resolves `aria-describedby`
   onto the focusable descendant.

Plus a tail: phone paste corrupted numbers (`+1 (555) 123-4567` → `+11555123456`); `ui-icon` was
`display: inline`, so `animate-spin` was a **no-op** (transforms don't apply to non-replaced inline
elements); 61 of 131 icon names silently resolved to an empty `<svg>`.

### The two vacuous gates

These deserve their own heading, because a green gate that cannot see is worse than no gate: it
actively buys false confidence.

- **A checker that shares a scanner with the thing it checks.** `gen-icon-registry.mjs`'s scanner
  regex listed `icon|iconName|leadingIcon|…` but not `iconEnd` — and `\bicon\b` cannot match
  `iconEnd`. So `<ui-button iconEnd="arrow-right">` was invisible to the **generator**, and equally
  invisible to `check-icons.mjs`, _which shares the scanner_. `lucideArrowRight` was never registered
  in admin, four 16×16 empty boxes shipped on the admin home page, and the blank-icon gate reported
  **green**. (Also missing: `actionIcon`, `badgeIcon`, `emptyIcon`, `ctaIcon`.)

- **A checker that cannot see untracked files.** `tools/codemods/lib/io.mjs`'s `listFiles()` used
  `git ls-files` — **tracked files only**. Every file a step _created_ was invisible to every gate
  until it was committed. S11 wrote a new table engine, a paginator and two vendored primitives;
  `check-icons`, `burndown` and `spartan-tokens` all reported green **having never read them**. The
  gates ran clean over precisely the newest, least-reviewed code in the migration. That is bug #8.
  Fixed with `--cached --others --exclude-standard`, and _proven_ non-vacuous by planting an
  untracked file with a bogus icon and watching the gate fail.

> Both have the same shape: **a checker that cannot see (or shares a scanner with) the thing it
> checks confirms only its own blind spots.** The new `phase7.mjs` gate is deliberately an
> _independent_ implementation — its own fs walk (not `git ls-files`), its own comment stripper (not
> `burndown`'s regex) — and it **self-tests against positive controls before it reports green**.

---

## What shipped, phase by phase

Phases 0–4 were the groundwork (Angular 22, then the safety net, then centralising every PrimeNG
touchpoint behind `ui-*` seams, then Signal Forms). Steps S0–S14 were the removal itself.

|             | What                                                                         | Real numbers                                                                                                                                                                                                                                                                            |
| ----------- | ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **P0**      | Spike: does PrimeNG 21 survive Angular 22 + CDK 22?                          | **Yes.** All 5 projects build. PrimeNG never touches CDK overlays, so the feared version skew did not exist — this de-risked the whole "upgrade first, swap later" ordering                                                                                                             |
| **P1**      | Angular 22 upgrade                                                           | 371 components, none declaring `changeDetection`. The v22 migration stamps `Eager` on every one — **reverted**; the app is zoneless and takes the new `OnPush` default. Router `paramsInheritanceStrategy` pinned back to `emptyOnly` (it silently flipped, with no migration provided) |
| **P2**      | Safety net                                                                   | The repo had **zero `.spec.ts` files**, no lint step, no test step in CI. Everything below rests on fixing that                                                                                                                                                                         |
| **P3**      | Seam hardening (still on PrimeNG)                                            | 82 table templates → one `<ui-data-table>`; 9 `ControlValueAccessor`s → `FormValueControl`; `BaseTable` deleted; duplicate `base-list.store.ts` collapsed. **This is what made the endgame cheap**                                                                                      |
| **P4**      | Signal Forms                                                                 | 52 components. One bug 46 times: `invalid` is bound from form _creation_, not first interaction, so every required-empty field rendered invalid on page load. Gated all wrappers on `invalid() && (touched() \|\| dirty())`                                                             |
| **S0**      | Guardrails                                                                   | burndown ratchet, budgets, `normalize-helm.mjs`, codemod lib, `.gitattributes`                                                                                                                                                                                                          |
| **S1**      | Characterization capture **against live PrimeNG**                            | The spec that made S11 survivable. It passed **unchanged** through the table swap — 2 changed lines, both a fixture selector rename. Zero `it()` bodies touched                                                                                                                         |
| **S2–S3**   | Icon runtime + 940-site sweep                                                | Typed `UiIconName` union → an unknown icon is now a **compile error**. `primeicons` dead                                                                                                                                                                                                |
| **S4**      | `ui-button`                                                                  | 498 sites                                                                                                                                                                                                                                                                               |
| **S5–S6**   | Cosmetics + tooltip                                                          | 608 + 124 sites                                                                                                                                                                                                                                                                         |
| **S7–S9**   | Dialogs, toasts, menus, tabs, accordion, popover, drawer, stepper, timeline  | 46 dialogs; `ToastService` onto sonner                                                                                                                                                                                                                                                  |
| **S10**     | Raw form controls, chart, editor, uploads                                    | 55 controls (not 6). Surface 466 → 85                                                                                                                                                                                                                                                   |
| **S11–S12** | **The crux.** `p-table` ripped out of `ui-data-table`; 7 engines hand-rolled | **83 of 90 consumer templates changed zero lines.** That was the entire safety argument for P3, and it held                                                                                                                                                                             |
| **S13**     | Theme port + removal (one-way door)                                          | See below                                                                                                                                                                                                                                                                               |
| **S14**     | CI exit gate + this document                                                 | `tools/gates/phase7.sh`                                                                                                                                                                                                                                                                 |

### S13 — the one thing in the theme that actually mattered

Deleting `providePrimeNG` was **not** a no-op, but not for the reason anyone expected. Zero elements
in the rendered DOM carried a `p-*` class, so every class-scoped rule was already dead. But the
injected sheet carried four rules that were **not** class-scoped, and one was load-bearing:

```css
:root,
:host {
  color-scheme: light;
}
.dark-theme {
  color-scheme: dark;
}
```

`providePrimeNG` was the **only source of `color-scheme` in the entire document**. Delete it and it
falls back to `normal` — native scrollbars, form controls and the page canvas silently stay **light in
dark mode**. Ported into `theme.css`. The other three rules were inert (`--p-*` vars nothing consumed;
a `box-sizing` reset Tailwind preflight already provides).

Verified by computed-style digest across 11 routes × light/dark: **1,089,432 comparisons, 22
differences — all `width`/`height`, zero colour/background/border/shadow/font drift.** Pixel diff:
16/22 surfaces byte-identical, including all of `/ui-lab` (7023 px tall) at **0 px**. The 4 that
differed were proven to differ _between two runs of identical code_ (an API 429 rendered a "Too many
requests" toast in the BEFORE capture) — **zero pixels attributable to PrimeNG removal.**

---

## Lessons

The ones that each cost a real bug. In rough order of how much they cost:

1. **If the brief contradicts the source, trust the source and say so loudly.** ~15 "verified facts"
   were wrong; the agent that checked was right every time. A fact derived from a grep of one
   spelling is not a fact.
2. **A checker that cannot see (or shares a scanner with) the thing it checks confirms only its own
   blind spots.** Two vacuous gates, both green, both hiding a shipped bug. Gates need positive
   controls: plant the bug, watch the gate fail, _then_ trust it.
3. **Read the library's compiled metadata and its DEFAULTS, not its docs and not its template.**
   Defaults invert (`allowEmpty`, `nullable`, `draggable`, `hideOnEscape`, tooltip `position: right`,
   toaster `bottom-right`).
4. **Never forward `undefined` for an input whose library default is not `undefined`.** This one bug
   class appeared five separate times and emptied every client-paginated table in the app.
5. **`focus`/`blur` and `scroll` do not bubble.** A wrapper element silently breaks any library that
   listens on its own host. (Use `focusin`/`focusout`.)
6. **An overlay must close, and a document-level Escape handler fires for nested overlays too.**
   That is a data-loss bug, not a UX nit.
7. **A wrapper element changes layout.** Keeping `<ui-button>` as a real node around `<button>` was
   right, but it is a node, and it has consequences (see 5).
8. **Centralise before you swap.** P3 pushed 82 tables and every form control behind `ui-*` seams
   _while still on PrimeNG_. That is why S11 changed 83 of 90 consumer templates by zero lines. The
   boring phase is the one that buys the endgame.
9. **Capture characterization tests against the OLD library, before you touch it.** S1's spec passed
   unchanged through the engine swap. Without it, S11 is unshippable.
10. **A metric that punishes documenting the thing you're removing cannot be used to remove it.**
    The gates exempt comments deliberately — the ~75 files of prose explaining _what was removed and
    why_ are the most valuable text in the repo. See the header of `phase7.mjs`.

---

## The gate — the definition of done

`tools/gates/phase7.sh`, wired into `.github/workflows/build.yml`, plus `bun run lint --max-warnings 0`.

```bash
bun run gate:phase7     # self-test → source scan → manifests → check:all cross-check
```

It scans `projects/` for PrimeNG in **code** — comments naming PrimeNG are exempt, deliberately and
loudly — and asserts `primeng` / `primeicons` / `@primeuix` are absent from the **Angular workspace**
`package.json` (not the repo-root one: that is a bun workspace root, has never listed them, and a
root-targeted assertion therefore passes _vacuously_), from `bun.lock`, and from `angular.json`.

Three things a future maintainer needs to know about it:

- **It is an independent scanner, on purpose.** It does not reuse `burndown.mjs`. Two independent
  implementations agreeing on 0 is evidence; one agreeing with itself is not (see the vacuous gates).
- **It self-tests before it reports.** 20 positive controls prove the comment stripper can still _see_
  real code. If the stripper ever fails open, CI fails on the gate rather than passing the repo.
- **Some greps cannot return literal zero, and should not.** `git grep -i primeng` still hits the 15
  migration codemods and `burndown.mjs`'s own metric regex — the tooling must name the thing it
  removes. Satisfying that literally means deleting the enforcement. The gate scopes to `projects/`
  and says so.

### A trap worth knowing: `bun install --force` does not prune

After PrimeNG was removed from `package.json` and `bun.lock`, `node_modules/primeng` was **still
physically present and resolvable** on the dev machine — `bun install --force` re-resolves but does
not garbage-collect. A stray `import { Button } from "primeng/button"` would still have **compiled
locally**, and only broken in CI (which installs into a fresh tree). Verified: after
`rm -rf node_modules && bun install`, `primeng` is `MODULE_NOT_FOUND` and the `.bun` store is clean.
The gate is what stands between a stale local tree and a broken build.

---

## Still open

- **`@ngrx/signals` has no Angular 22 release** (latest 21.1.1, peer `^21.0.0`). It works only
  because bun does not enforce peers, and it backs `base-list.store.ts`. **The most likely thing to
  break on Angular 23.**
- **spartan is code-in-repo.** Upstream fixes do not flow automatically. `tools/normalize-helm.mjs`
  strips CVAs from vendored primitives on the way in — keep running it, and keep reading what you
  vendor (that is how `NG_VALUE_ACCESSOR` got back in once already).
- **The website eagerly bundles sonner + CDK overlay + Helm** (712 KB initial chunk) because its root
  template renders `<ui-toaster/>` and a dialog. Legitimate, and still below pre-migration — but it is
  the biggest remaining lever if the marketing site needs to be slimmer.
- **`angular-gridster2` legitimately requires `::ng-deep`** (`home.css`). The gate bans `::ng-deep` _at
  a `.p-_`class*, not`::ng-deep` itself. Do not "fix" that.
