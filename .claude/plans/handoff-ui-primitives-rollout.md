# Handoff: UI Primitives Rollout — Resume Refactor (TMS leftovers + Admin + Customer)

> **Last updated 2026-04-30 after 12 commits.** Jobs 1 + 2 are complete and committed. Job 3 (mass page refactor) is partially done — most TMS dashboards/lists/details are converted; admin home + 3 customer-portal pages are converted; the rest remains. Resume from the **Resume here** section below.

## Context

A MUI-inspired primitive layer was added to `@logistics/shared` in [the parent plan](C:\Users\admin.claude\plans\okay-scan-tms-portal-admin-portal-replicated-pearl.md). The primitives ship as `<ui-typography>`, `<ui-stack>`, `<ui-grid>`, `<ui-surface>`, `<ui-container>`, `<ui-divider>`, `<ui-icon>`, `<ui-badge>`, `<ui-status-badge>`, `<ui-callout>`, `<ui-toolbar>`, `<ui-action-menu>` from `@logistics/shared/components`.

## Status Summary

### Jobs 1 + 2 — DONE ✅

- **Folder restructure**: shared lib reorganised into `inputs/`, `data-display/`, `feedback/` (was `form/`, `domain-forms/`, `ui/`, `state/`, `other/`).
- **Renames**: `LabeledField` → `FormField` (selector `ui-labeled-field` → `ui-form-field`); `BaseTableComponent` → `BaseTable`. Both bundled into commit `289d6fa5`.

### Job 3 — Partially complete

#### TMS portal — converted (committed)

- **Dashboards / stats**: `dashboard`, `home`, `invoices/invoice-dashboard`, `eld/eld-dashboard`, `payroll/dashboard`, `maintenance/maintenance-dashboard`
- **Reports** (all six): `loads-report`, `financials-report`, `payroll-report`, `safety-report`, `maintenance-report`, `drivers-report/drivers-dashboard`
- **List / detail pages**: `loads/loads-list`, `loads/load-detail`, `containers/containers-list`, `customers/customers-list`, `customers/customer-details`, `employees/employees-list`, `employees/employee-details`, `terminals/terminals-list`, `trucks/trucks-list`, `trucks/truck-details`
- **Other**: `eld/eld-hos-logs`, `eld/eld-driver-mappings`, `safety/dvir-list`, `safety/accidents-list`, `settings/ai-settings`, `settings/company-settings`, `payroll/invoices/details/payroll-invoice-details`, `invoices/load-invoice-details`

**Side cleanups**: dropped duplicated `getStatusSeverity` / `getStatusLabel` helpers in `home.ts`, `containers-list.ts`, `loads-report.ts`, plus the customer-portal versions (`dashboard.ts`, `shipments-list.ts`, `invoices-list.ts`).

#### Admin portal — converted

- `home/home` (dashboard)

#### Customer portal — converted

- `dashboard/dashboard`, `shipments/shipments-list`, `invoices/invoices-list` (the highest dark-mode-win pages — hardcoded `bg-white` cards swapped to `<ui-surface>`).

### Build status

`bun --bun ng build tms-portal`, `bun --bun ng build admin-portal`, `bun --bun ng build customer-portal` were all clean at the last commit. Lint is unchanged (still 27 pre-existing `admin-`-selector errors, unrelated to this work).

## Resume here

### TMS portal — remaining

Working dir: `src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/`

Confirmed pages still using `grid grid-cols-12` / `grid-cols-1 md:grid-cols-2` / `<p-tag [severity]>` / hardcoded card divs (run a fresh `Grep` to verify before each pass):

| Folder                                        | Page(s)                                                                                                                                                                                                                                      |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `trips/`                                      | `trips-list`, `trip-add`, `trip-edit`, `trip-detail`, `components/trip-wizard-*`                                                                                                                                                             |
| `expenses/`                                   | `expenses-list`, `expense-add`, `expense-edit`                                                                                                                                                                                               |
| `maintenance/`                                | `service-records`, `service-record-detail`, `service-record-add`, `service-record-edit`, `upcoming-service`                                                                                                                                  |
| `payroll/`                                    | `invoices/list`, `invoices/add`, `invoices/edit`, `employee/employee-payroll-invoices`                                                                                                                                                       |
| `invoices/`                                   | `employee-payroll-invoices-list`, `load-invoices-list`, components/`payment-link-dialog`, components/`send-invoice-dialog`, components/`record-payment-dialog` (dialogs already use `<ui-form-field>`; mostly `<ui-stack>` for inner layout) |
| `safety/`                                     | `dvir-detail`, `condition-reports-list`, `condition-report-detail`, `accident-detail`, `driver-behavior-list`, `components/dvir-defects-list`, `components/accident-review-summary`                                                          |
| `eld/`                                        | `eld-providers`                                                                                                                                                                                                                              |
| `settings/`                                   | `api-keys-settings`, `feature-settings`, `payment-settings`, `_components/api-keys-table`                                                                                                                                                    |
| `subscription/`                               | `view-plans`, `manage-subscription`                                                                                                                                                                                                          |
| `load-board/`                                 | `posted-trucks`, `load-board-search`, `load-board-providers`                                                                                                                                                                                 |
| `loads/`                                      | `load-add`, `load-edit`, `load-import`, `components/loads-filter-panel`, `components/loads-table` (subcomponents — main `loads-list` + `load-detail` are already done)                                                                       |
| `trucks/`                                     | `truck-edit`, `truck-documents`, `components/trucks-filter-panel`                                                                                                                                                                            |
| `customers/`                                  | `customer-add`, `customer-edit`, `components/invite-customer-dialog`, `components/customer-edit-dialog`                                                                                                                                      |
| `employees/`                                  | `employee-add`, `employee-edit`, `employee-invitations-list`, `components/invite-employee-dialog`, `components/employee-edit-dialog`                                                                                                         |
| `containers/`                                 | `container-edit` (form is already done — only minor consistency tweaks)                                                                                                                                                                      |
| `terminals/`                                  | `terminal-add`, `terminal-edit`, `terminal-form`                                                                                                                                                                                             |
| `timesheets/`                                 | `list/timesheets-list`, `components/form-dialog`                                                                                                                                                                                             |
| `notifications/`, `messages/`, `ai-dispatch/` | various — lower priority                                                                                                                                                                                                                     |

### Admin portal — remaining

Working dir: `src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/`

| Folder                 | Page(s)                                                         |
| ---------------------- | --------------------------------------------------------------- |
| `tenants/`             | `tenants-list`, `tenant-edit`, `tenant-add`, `tenant-quotas`    |
| `plans/`               | `plans-list`, `plan-edit`, `plan-add`                           |
| `subscriptions/`       | `subscriptions-list`, `subscription-detail`, `subscription-add` |
| `users/`               | `users-list`, `user-detail`                                     |
| `blog-posts/`          | `blog-posts-list`, `blog-post-add`, `blog-post-edit`            |
| `demo-requests/`       | `demo-requests-list`                                            |
| `contact-submissions/` | `contact-submissions-list`                                      |

Many of these wrap content directly in `<p-card>` — replace ad-hoc styled boxes with `<ui-surface>`, but keep `<p-card>` where header/sub-header slots are in use.

### Customer portal — remaining

Working dir: `src/Client/Logistics.Angular/projects/customer-portal/src/app/pages/`

| Folder                                            | Page(s)                                                             |
| ------------------------------------------------- | ------------------------------------------------------------------- |
| `account/`                                        | `account-settings`                                                  |
| `documents/`                                      | `documents-list`                                                    |
| `tracking/`                                       | `public-tracking` (heavy hardcoded `bg-white` — high dark-mode-win) |
| `shipments/`                                      | `shipment-details`                                                  |
| `payment/`                                        | `public-payment`                                                    |
| `login/`, `select-tenant/`, `errors/unauthorized` | small pages                                                         |

## Primitive API Quick Reference

### `<ui-typography>`

```html
<ui-typography variant="h2" color="primary" weight="bold" align="start" tag="h1">
  Page title
</ui-typography>
```

- `variant`: `h1`–`h6` | `body` | `body-sm` | `caption` | `overline` | `label` | `stat`. Default `body`.
- `color`: `primary` | `secondary` | `muted` | `inherit`.
- **`tag` is the prop name, not `as`** — `as` is reserved in Angular template expressions.

### `<ui-stack>`

```html
<ui-stack direction="row" gap="3" align="center" justify="between" wrap tag="section">
  ...
</ui-stack>
```

- `direction`: `row` | `col`. Default `col`.
- `gap`: string `"0"`–`"8"`. Default `"4"`.
- **String-typed**: `gap="3"`, not `[gap]="3"`. Booleans (`wrap`, `border`, `container`) accept bare-attribute syntax.

### `<ui-surface>`

```html
<ui-surface variant="elevated" padding="md" radius="lg" [border]="true">...</ui-surface>
```

- `variant`: `elevated` | `subtle` | `plain`. Default `elevated`.
- `padding`: `none` | `sm` | `md` | `lg`. Default `md`.

### `<ui-container>`

- `maxWidth`: `xs` | `sm` (`max-w-3xl`) | `md` (`max-w-5xl`) | `lg` (`max-w-7xl`) | `xl` | `full`. Default `lg`.
- Refactor checklist: `max-w-5xl` → `maxWidth="md"`, `max-w-7xl` → `maxWidth="lg"`.

### `<ui-grid>` (MUI v7 style)

```html
<ui-grid container spacing="4">
  <ui-grid [size]="{ xs: 12, md: 6 }">...</ui-grid>
  <ui-grid [size]="6">...</ui-grid>
  <ui-grid [size]="'auto'">...</ui-grid>
</ui-grid>
```

### `<ui-status-badge>`

```html
<ui-status-badge [status]="load.status" kind="load" />
```

- `kind`: `load` | `truck` | `container` | `subscription` | `invoice` | `employee`. Severity is auto-resolved from status string via [severity-maps.ts](../../src/Client/Logistics.Angular/projects/shared/src/lib/components/primitives/status-badge/severity-maps.ts).

### `<ui-toolbar>`, `<ui-action-menu>`, `<ui-callout>`

See the inline examples in `dashboard.html`, `containers/container-add.html`, etc. for live usage.

## Per-Page Recipe (mechanical)

Apply in this order; rebuild after each page:

1. **Imports**: add to `@Component({ imports: [...] })` whichever primitives the template uses. Most pages need `Grid, Icon, Stack, Typography`; details pages also `Surface`; lists with status need `StatusBadge`.

   ```ts
   import { Grid, Icon, Stack, Surface, Typography } from "@logistics/shared/components";
   ```

2. **Header `<h1>` + tagline `<p>`** in animated wrappers → `<ui-typography variant="h2" tag="h1">`/`variant="body" color="muted"`.

3. **Outer `flex items-center justify-between` rows** → `<ui-stack direction="row" align="center" justify="between">`.

4. **`grid grid-cols-12 gap-N`** → `<ui-grid container spacing="N">`. Children with `col-span-12 md:col-span-6 lg:col-span-3` → `<ui-grid [size]="{ xs: 12, md: 6, lg: 3 }">`.

5. **`grid grid-cols-1 md:grid-cols-2` form layouts** → `<ui-grid container spacing="4">` + `<ui-grid [size]="{ xs: 12, md: 6 }">`.

6. **`<div class="border-default bg-elevated rounded-lg border p-4">`** info banners → `<ui-surface variant="elevated" [border]="true">`.

7. **`<p-tag [severity]="getXxxSeverity(item.status)">`** with kind-style status → `<ui-status-badge [status]="item.status" kind="...">`. Delete the helper.

8. **Bare `<i class="pi pi-*">`** when size/color matters → `<ui-icon name="..." [size]="..." [color]="...">`.

9. **Loading wrapper `<div class="flex justify-center py-N">`** → `<ui-stack align="center" justify="center" class="py-N">`.

10. **Empty/error states** with stacked icon+text → `<ui-stack align="center" gap="2/3/4" class="py-N">`.

## Build & Verify per Commit

```bash
cd src/Client/Logistics.Angular
bun --bun ng build tms-portal       # or admin-portal / customer-portal
bun --bun ng build admin-portal
bun --bun ng build customer-portal
```

Note: bun's resolver is finicky — use `bun --bun ng build <project>` (not `bun ng` or `npm run build`).

## Pitfalls to Avoid

- **String-typed inputs** (`gap`, `spacing`): `gap="3"` (literal attribute), not `[gap]="3"`.
- **`tag` not `as`**: the semantic-tag override is named `tag`.
- **Closing tags after div→stack/grid swap**: when changing `<div class="flex ...">` to `<ui-stack>`, **always rewrite the matching `</div>` to `</ui-stack>`**. Same for `</ui-grid>`. Several mid-edit build failures during the TMS rollout were tag-mismatch errors at the bottom of large files — search for stray `</div>` after a wrap-rewrite.
- **`replace_all` swaps**: be careful — there are usually multiple `<div class="flex items-center gap-2">` patterns with different children. Prefer surgical Edits over replace_all on common class strings.
- **Add primitives to the `imports: [...]` array** in the `.ts`, not just to the import line. Several "unknown element" build errors during this work came from leaving the import unused.
- **Specialized severity helpers** (e.g. `getDutyStatusSeverity` for ELD logs, `getSeveritySeverity` for accident severity, `getDvirTypeSeverity`) don't fit the `StatusKind` enum. Leave those as `<p-tag [severity]>` — only swap when status maps to a known `kind`.
- **No theme tokens**: don't introduce new colors. Tailwind utility classes like `text-success`, `text-danger`, `bg-blue-600/10` are fine on existing pages.
- **No behavior changes**: refactor PRs are visual-parity only.
- **Dark-mode check** on customer-portal pages — easiest place to spot hardcoded colors.
- **Stale IDE diagnostics**: after fixing imports, the IDE will sometimes still show "unknown element" errors. The CLI build is the source of truth.

## Commit Style

Follow the established session pattern — conventional-commits header + a short body listing the touched pages and the dominant transformations. Examples:

```
refactor(tms-portal): convert eld + safety list pages to primitives

- eld-hos-logs: header + summary cards (grid-cols-12) + filter + empty/error
  states → ui-stack/ui-grid/ui-typography/ui-icon
- eld-driver-mappings: header + mappings/create-mapping cards + form layout
  (grid-cols-12) → ui-stack/ui-grid/ui-typography/ui-icon
- dvir-list: defects-count column → ui-stack + ui-icon
- accidents-list: injuries-flag column → ui-stack + ui-icon
```

One commit per logical batch of ~4–8 files keeps each diff reviewable.

## Estimated Remaining Effort

| Scope                                                                                                                          | Files | Effort    |
| ------------------------------------------------------------------------------------------------------------------------------ | ----- | --------- |
| TMS leftovers (trips, expenses, maintenance, payroll list/edit, settings 3, safety details, load-board, subscription, dialogs) | ~50   | ~1 day    |
| Admin portal                                                                                                                   | ~20   | ~half day |
| Customer portal                                                                                                                | ~7    | ~2 hours  |

Total: ~2 focused days across 3–6 PRs, similar cadence to what's already shipped.

## Reference Pages (good examples)

For the patterns above, look at how these committed pages handle the tricky cases:

- **Stat-card grid**: [tms-portal/dashboard.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/dashboard/dashboard.html)
- **Detail page with tabs + info cards**: [tms-portal/customer-details.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/customers/customer-details/customer-details.html), [employee-details.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/employees/employee-details/employee-details.html), [truck-details.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/trucks/truck-details/truck-details.html)
- **Form proof page**: [container-add.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/containers/container-add/container-add.html)
- **List with status-badge swap**: [containers-list.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/containers/containers-list/containers-list.html)
- **Two-column invoice layout**: [load-invoice-details.html](../../src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/invoices/load-invoice-details/load-invoice-details.html)
- **Customer portal dark-mode swap**: [customer-portal/dashboard.html](../../src/Client/Logistics.Angular/projects/customer-portal/src/app/pages/dashboard/dashboard.html)
