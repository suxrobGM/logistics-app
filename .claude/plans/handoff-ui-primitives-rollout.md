# Handoff: UI Primitives Rollout — Complete the Restructure & Replace Hardcoded Patterns

## Context

A MUI-inspired primitive layer was added to `@logistics/shared` in [the parent plan](C:\Users\admin.claude\plans\okay-scan-tms-portal-admin-portal-replicated-pearl.md). The primitives + one proof page have shipped. Three follow-up jobs remain:

1. **Folder restructure** of `projects/shared/src/lib/components/` (deferred to keep the primitives PR low-risk).
2. **Rename `ui-labeled-field` → `ui-form-field` and `BaseTableComponent` → cleaner naming** (touches ~104 files; deferred).
3. **Mass page refactor** — replace inline Tailwind class strings with primitives across all three portals.

Recommended order: **Job 1 → Job 2 → Job 3**, one PR each.

Reference plan: `C:\Users\admin\.claude\plans\okay-scan-tms-portal-admin-portal-replicated-pearl.md`

## What's Already Shipped

**New primitives** in `projects/shared/src/lib/components/primitives/`, exported from `@logistics/shared`:

| Selector            | Class         | Purpose                                                               |
| ------------------- | ------------- | --------------------------------------------------------------------- |
| `<ui-typography>`   | `Typography`  | Headings, body, label, caption, overline, stat — variant-driven       |
| `<ui-stack>`        | `Stack`       | Flex container (row/col) with gap/align/justify/wrap                  |
| `<ui-surface>`      | `Surface`     | Themed card/section background (elevated/subtle/plain)                |
| `<ui-container>`    | `Container`   | Page-width wrapper with `mx-auto` + `maxWidth` (xs/sm/md/lg/xl/full)  |
| `<ui-divider>`      | `Divider`     | Horizontal/vertical separator with optional inline label              |
| `<ui-icon>`         | `Icon`        | PrimeIcons wrapper with size/color variants                           |
| `<ui-badge>`        | `Badge`       | Generic tag with severity + variant                                   |
| `<ui-status-badge>` | `StatusBadge` | Auto-severity from `status` + `kind` enum                             |
| `<ui-grid>`         | `Grid`        | MUI v7 12-column responsive grid (single component, `container` flag) |
| `<ui-callout>`      | `Callout`     | Themed info/success/warning/danger alert box                          |
| `<ui-toolbar>`      | `Toolbar`     | Page action bar with start/center/end slots                           |
| `<ui-action-menu>`  | `ActionMenu`  | `pi-ellipsis-v` row context menu wrapping `<p-menu>`                  |

Severity-resolution helper: [`resolveStatusSeverity(kind, status)`](../../src/Client/Logistics.Angular/projects/shared/src/lib/components/primitives/status-badge/severity-maps.ts) — single source of truth for status → severity mapping. Edit the maps there to add/adjust statuses.

**Proof page refactored** (use as reference for the rollout):

- [container-add.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/containers/container-add/container-add.html)

## Primitive API Quick Reference

### `<ui-typography>`

```html
<ui-typography variant="h2" color="primary" weight="bold" align="start" tag="h1">
  Page title
</ui-typography>
```

- `variant`: `h1` | `h2` | ... | `h6` | `body` | `body-sm` | `caption` | `overline` | `label` | `stat`. Default `body`.
- `color`: `primary` | `secondary` | `muted` | `inherit`. Default `inherit`.
- `weight`, `align`, `tag` are optional overrides. **`tag` is the prop name, not `as`** — `as` is reserved in Angular template expressions.

### `<ui-stack>`

```html
<ui-stack direction="row" gap="3" align="center" justify="between" wrap tag="section">
  ...
</ui-stack>
```

- `direction`: `row` | `col`. Default `col`.
- `gap`: string `"0"`–`"8"` (subset: 0,1,2,3,4,6,8). Default `"4"`.
- `align`, `justify`: optional. `tag`: `div` | `section` | `header` | `footer`.
- **Important**: `gap`, `align`, `justify` are **string-typed**. Use literal attribute syntax (`gap="3"`), not `[gap]="3"`. Booleans (`wrap`, `border`, `container`, `dismissible`, `sticky`) accept the bare-attribute form thanks to `booleanAttribute` transforms.

### `<ui-surface>`

```html
<ui-surface variant="elevated" padding="md" radius="lg" [border]="true"> ... </ui-surface>
```

- `variant`: `elevated` (`bg-elevated`) | `subtle` (`bg-subtle`) | `plain`. Default `elevated`.
- `padding`: `none` | `sm` | `md` | `lg`. Default `md`.

### `<ui-container>`

```html
<ui-container maxWidth="lg">...</ui-container>
<ui-container maxWidth="md" [gutters]="false">...</ui-container>
```

- `maxWidth`: `xs` (`max-w-md`) | `sm` (`max-w-3xl`) | `md` (`max-w-5xl`) | `lg` (`max-w-7xl`) | `xl` (`max-w-screen-xl`) | `full` (no cap). Default `lg`.
- `gutters`: bool, default `true` (adds `px-4 sm:px-6 lg:px-8`). Set `[gutters]="false"` if the parent layout already pads.

Replaces the ad-hoc `<div class="mx-auto max-w-5xl">` / `max-w-7xl` page wrappers. **Refactor checklist for pages**: `max-w-5xl` → `maxWidth="md"`, `max-w-7xl` → `maxWidth="lg"`.

### `<ui-grid>` (MUI v7 style)

```html
<!-- Container -->
<ui-grid container spacing="4">
  <!-- Item -->
  <ui-grid [size]="{ xs: 12, md: 6 }">...</ui-grid>
  <ui-grid [size]="6">...</ui-grid>
  <ui-grid [size]="'auto'">...</ui-grid>
</ui-grid>
```

- Container: `[container]="true"`, `spacing` (or `rowSpacing`/`columnSpacing`), `direction`, `wrap`.
- Item (no `container`): `[size]` accepts `number | 'auto' | 'grow' | { xs?, sm?, md?, lg?, xl? }`. `[offset]` accepts `number | breakpoint object`.

### `<ui-status-badge>`

```html
<ui-status-badge [status]="load.status" kind="load" />
```

- `kind`: `load` | `truck` | `container` | `subscription` | `invoice` | `employee`.
- Severity is resolved automatically from the status string.

### `<ui-toolbar>`

```html
<ui-toolbar>
  <ui-search-input slot="start" ... />
  <p-button slot="end" label="Add" ... />
</ui-toolbar>
```

### `<ui-action-menu>`

```html
<ui-action-menu [items]="rowActions(row)" />
```

```ts
rowActions(row: LoadDto): ActionMenuItem[] {
  return [
    { label: "Edit", icon: "pencil", action: () => this.edit(row) },
    { label: "Delete", icon: "trash", danger: true, action: () => this.delete(row) },
  ];
}
```

### `<ui-callout>`

```html
<ui-callout intent="warning" title="Heads up"> Your subscription expires in 3 days. </ui-callout>
```

---

## Job 1 — Folder Restructure

Move the existing component folders into a clearer role-based layout. Net result:

```
projects/shared/src/lib/components/
├── primitives/        (already exists — keep as-is)
├── inputs/            (NEW — was form/ + domain-forms/)
├── data-display/      (NEW — was ui/ + other/)
├── feedback/          (NEW — was state/ + ui/confirm-delete-dialog)
└── permission/        (unchanged)
```

### Move Plan

| From                                    | To                                           |
| --------------------------------------- | -------------------------------------------- |
| `components/form/labeled-field/`        | `components/inputs/labeled-field/`           |
| `components/form/currency-input/`       | `components/inputs/currency-input/`          |
| `components/form/phone-input/`          | `components/inputs/phone-input/`             |
| `components/form/search-input/`         | `components/inputs/search-input/`            |
| `components/form/unit-input/`           | `components/inputs/unit-input/`              |
| `components/form/validation-summary/`   | `components/inputs/validation-summary/`      |
| `components/domain-forms/address-form/` | `components/inputs/address-form/`            |
| `components/ui/page-header/`            | `components/data-display/page-header/`       |
| `components/ui/stat-card/`              | `components/data-display/stat-card/`         |
| `components/ui/dashboard-card/`         | `components/data-display/dashboard-card/`    |
| `components/ui/pdf-viewer/`             | `components/data-display/pdf-viewer/`        |
| `components/other/date-range-picker/`   | `components/data-display/date-range-picker/` |
| `components/other/base-table/`          | `components/data-display/base-table/`        |
| `components/state/data-container/`      | `components/feedback/data-container/`        |
| `components/state/empty-state/`         | `components/feedback/empty-state/`           |
| `components/state/error-state/`         | `components/feedback/error-state/`           |
| `components/state/loading-skeleton/`    | `components/feedback/loading-skeleton/`      |
| `components/ui/confirm-delete-dialog/`  | `components/feedback/confirm-delete-dialog/` |

**Optionally** also move toolbar + action-menu:
| `components/primitives/toolbar/` | `components/data-display/toolbar/` |
| `components/primitives/action-menu/` | `components/data-display/action-menu/` |

### Steps

1. Use `git mv` for each folder (preserves history). Bash example:

   ```bash
   cd projects/shared/src/lib/components
   mkdir -p inputs data-display feedback
   git mv form/labeled-field inputs/
   git mv form/currency-input inputs/
   git mv form/phone-input inputs/
   git mv form/search-input inputs/
   git mv form/unit-input inputs/
   git mv form/validation-summary inputs/
   git mv domain-forms/address-form inputs/
   git mv ui/page-header data-display/
   git mv ui/stat-card data-display/
   git mv ui/dashboard-card data-display/
   git mv ui/pdf-viewer data-display/
   git mv other/date-range-picker data-display/
   git mv other/base-table data-display/
   git mv state/data-container feedback/
   git mv state/empty-state feedback/
   git mv state/error-state feedback/
   git mv state/loading-skeleton feedback/
   git mv ui/confirm-delete-dialog feedback/
   rmdir form domain-forms ui state other
   ```

2. Replace `components/index.ts` with the new barrels:

   ```ts
   export * from "./permission/permission-guard";
   export * from "./permission/permission-checker";
   export * from "./primitives";
   export * from "./inputs";
   export * from "./data-display";
   export * from "./feedback";
   ```

3. Create new per-folder `index.ts` files mirroring the contents of the old ones (`form/index.ts`, `state/index.ts`, etc.) — same exports, same surface.

4. **No consumer code changes needed.** Consumers import from `@logistics/shared` or `@logistics/shared/components`, both of which keep working through the top-level barrel.

5. Verify:
   ```bash
   bun run build:all
   bun run lint
   ```

---

## Job 2 — Renames

### `ui-labeled-field` → `ui-form-field`

~52 HTML files use `<ui-labeled-field>`, ~52 TS files import `LabeledField`. Mechanical find/replace.

**Steps:**

1. Rename the file + class:
   - `inputs/labeled-field/labeled-field.ts` → `inputs/form-field/form-field.ts`
   - `inputs/labeled-field/labeled-field.html` → `inputs/form-field/form-field.html`
   - Class `LabeledField` → `FormField`
   - Selector `ui-labeled-field` → `ui-form-field`
2. Update barrel export.
3. Mass find/replace across the workspace:
   ```bash
   # PowerShell (Windows)
   Get-ChildItem -Recurse -Include *.html,*.ts | ForEach-Object {
     (Get-Content $_) -replace 'ui-labeled-field', 'ui-form-field' `
                     -replace '\bLabeledField\b', 'FormField' |
       Set-Content $_
   }
   ```
   Run from `src/Client/Logistics.Angular/`. **Exclude** `node_modules/`, `dist/`, `.angular/`. Verify with `git diff`.
4. Build and lint.

### `BaseTableComponent` → keep as-is or rename to `BaseTable`

`BaseTableComponent` is an **abstract inheritance base**, not a UI element. There is no `<ui-base-table>` selector in any template. It's used via `class XYZ extends BaseTableComponent<T>`.

**Recommendation**: rename class to `BaseTable` (drop `Component` suffix per the project's no-`Component`-suffix convention) — affects ~5 files (already enumerated in `git grep BaseTableComponent`). Folder stays `data-display/base-table/`. Selector concerns don't apply.

**Alternative**: leave it as `BaseTableComponent`. The "rename `ui-base-table` → `ui-table`" line in the parent plan was based on a faulty premise (no such selector exists). Skip the rename, accept the slight naming inconsistency.

---

## Job 3 — Mass Page Refactor

Order: **TMS portal → Admin portal → Customer portal**. One PR per portal. Within a portal, group by feature folder.

### TMS Portal (~30 pages)

Working dir: `src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/`

Highest-impact targets:

- `loads/` — list, detail, form
- `trucks/` — list, detail, form
- `drivers/` & `employees/` — list, detail, form
- `customers/` — list, detail, form
- `containers/` — list + detail (form already done as proof)
- `terminals/` — list, form
- `dashboard/` — heavy `grid grid-cols-12 gap-4` use → `<ui-grid container>`
- `trips/`, `invoices/`, `payments/`, `payroll/`, `documents/`, `settings/*`

### Admin Portal (~20 pages)

Working dir: `src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/`

Highest-impact:

- `tenants/` — tenant-edit, tenant-quotas
- `plans/` — list + editor
- `subscriptions/` — list + detail
- `users/` — list + detail
- `home/` (dashboard) — stat-card grid → `<ui-grid>`
- `blog-posts/`, `demo-requests-list/`

Many admin pages use `<p-card>` directly → replace with `<ui-surface>` for theme consistency.

### Customer Portal (~10 pages)

Working dir: `src/Client/Logistics.Angular/projects/customer-portal/src/app/pages/`

**Highest impact + biggest dark-mode-correctness wins** (this portal currently uses hardcoded `bg-white`):

- `dashboard/` — hardcoded `rounded-lg bg-white p-6 shadow` cards
- `account-settings/`
- `shipments-list/` and timeline
- `invoices/`

### Per-Page Checklist

For each page:

1. Replace `<div class="border-default bg-elevated rounded-lg border p-4">` → `<ui-surface>`.
2. Replace `<div class="flex {row|col} gap-N items-* justify-*">` → `<ui-stack>`.
3. Replace styled `<h*>` / `<span>` / `<p>` → `<ui-typography>`.
   - Section titles (uppercase tracking-wide) → `variant="overline" color="secondary"`.
4. Replace `grid grid-cols-1 md:grid-cols-2` form layouts → `<ui-grid container spacing="4">` with `<ui-grid [size]="{ xs: 12, md: 6 }">` items.
5. Replace `<p-tag [severity]="getXxxSeverity()">` → `<ui-status-badge [status]="..." kind="...">`. Delete the per-component `getXxxSeverity()` method.
6. Replace `mb-4 flex items-center justify-between` toolbars above tables → `<ui-toolbar>` with `slot="start"` / `slot="end"`.
7. Replace `<p-button icon="pi pi-ellipsis-v" text>` + adjacent `<p-menu>` → `<ui-action-menu [items]="...">`.
8. Replace `bg-subtle border` info/warning boxes → `<ui-callout intent="...">`.
9. Replace bare `<i class="pi pi-*">` → `<ui-icon name="...">` where size/color variants matter.
10. **Visual-diff in light AND dark mode** before opening the PR.

---

## Verification per PR

```bash
cd src/Client/Logistics.Angular
bun run build:all
bun run lint

# Spot-check pages in light + dark mode (toggle theme via portal header)
bun run start:tms       # https://localhost:7003
bun run start:admin     # https://localhost:7002
bun run start:customer  # https://localhost:7004
```

No new tests required — refactors are visual-parity only.

## Pitfalls to Avoid

- **String-typed inputs** (`gap`, `spacing`, etc.): use `gap="3"` (literal attribute), not `[gap]="3"`. The latter passes a number, which fails the string-union typecheck.
- **`tag` not `as`**: `as` is a reserved keyword in Angular template expressions (used in `*ngIf="x as y"` syntax). The semantic-tag override on Stack/Typography is named `tag`.
- **Do NOT introduce new theme tokens.** All primitives consume the existing CSS variables in `projects/tms-portal/src/styles/variables.css`. If a page needs a color the tokens don't have, that's a separate design-system PR.
- **Do NOT change behavior** in refactor PRs. Pure visual swaps only.
- **Do NOT skip dark-mode verification** on customer-portal pages — most likely to have hidden hardcoded colors.
- **Watch for class strings inside `[ngClass]` / `[class.xxx]` bindings** — these may need to migrate into primitive input bindings, not disappear.
- **`<p-card>` direct usage** — many admin pages use it. If just a styled box, replace with `<ui-surface>`. If using PrimeNG-specific features (header slot, sub-header), keep it.
- **`ui-labeled-field` template still has hardcoded `dark:text-red-400` etc.** — leave as-is during the rename; cleanup is a separate concern.

## Estimated Scope

| Job                                           | Effort                                                |
| --------------------------------------------- | ----------------------------------------------------- |
| 1: Folder restructure                         | ~1 hour (mostly mechanical `git mv` + barrel updates) |
| 2: `ui-form-field` rename + mass find/replace | ~2 hours (104 files, scriptable)                      |
| 3a: TMS portal refactor                       | ~1 day                                                |
| 3b: Admin portal refactor                     | ~half day                                             |
| 3c: Customer portal refactor                  | ~few hours                                            |

Total: ~3 focused days across 5 PRs (1 + 1 + 3 portal PRs).
