# Handoff: PrimeIcons → Lucide Icon Migration

> **Last updated 2026-05-01.** Spike complete. TMS sidebar + admin sidebar fully migrated as reference implementation. Approach validated. Remaining work: ~600 occurrences across ~250 files in TMS pages, admin pages, customer-portal, website, and the shared lib's own components.

## Why we're doing this

PrimeIcons (`primeicons` v7) is effectively unmaintained — last meaningful release was ~2 years ago, and the ~250-icon set keeps causing coverage gaps when adding new features. Lucide ships 1500+ icons with active development and tree-shaking.

## Approach (validated)

**Option B — Shared base + portal-specific extensions.** Each portal calls `provideLucideIcons(...BASE_LUCIDE_ICONS, ...PORTAL_LUCIDE_ICONS)` in its `app.config.ts`. `BASE_LUCIDE_ICONS` lives in `@logistics/shared` and contains foundational icons that the shared lib relies on plus icons every portal needs (chevrons, x, plus, search, arrows, log-out, trash, etc.). Portal-specific lists only contain icons unique to that portal.

Trade-off chosen: a tiny bit of indirection in exchange for guaranteed registration of shared-lib icons across all portals (a missing registration is a silent broken-icon at runtime, which is the worst failure mode).

## What's already done

### Foundation (committed in spike)

- **`@lucide/angular@1.14.0`** added to root `package.json` / `bun.lock`
- **`projects/shared/src/lib/icons/lucide-icons.ts`** — `BASE_LUCIDE_ICONS` array (16 icons): arrow-left, chevron-{up,down,left,right}, circle-plus, ellipsis-vertical, external-link, file-text, log-out, plus, refresh-cw, search, trash-2, triangle-alert, x
- **`projects/shared/src/public-api.ts`** — exports `BASE_LUCIDE_ICONS`

### TMS portal (sidebar only)

- **`projects/tms-portal/src/app/shared/icons/lucide-icons.ts`** — `TMS_LUCIDE_ICONS` (18 icons unique to TMS)
- **`app.config.ts`** — wired up
- **Sidebar migrated**: `sidebar-items.ts`, `nav-menu/nav-menu.{html,ts,css}`, `sidebar/favorites-bar/favorites-bar.{html,ts}`

### Admin portal (sidebar only)

- **`projects/admin-portal/src/app/shared/icons/lucide-icons.ts`** — `ADMIN_LUCIDE_ICONS` (10 icons)
- **`app.config.ts`** — wired up
- **Sidebar migrated**: `sidebar-items.ts`, `sidebar/sidebar.{html,ts}` (including a `<p-button icon=>` → `pTemplate="icon"` conversion)

### Build status (last verified)

- `bun run build:tms` — clean (8.6s)
- `bun run build:admin` — clean (6.6s)

## What's left

Roughly **600 occurrences across ~250 files**. By portal:

| Portal                         | Files          | Notes                                                                                                                     |
| ------------------------------ | -------------- | ------------------------------------------------------------------------------------------------------------------------- |
| `tms-portal` (pages)           | ~150           | Pages, dialogs, table action menus, badges, command palette                                                               |
| `admin-portal` (pages)         | ~30            | Pages, forms, mobile drawer/header, login                                                                                 |
| `customer-portal`              | ~10            | Public-tracking, shipments, navbar, layout                                                                                |
| `website`                      | ~5             | Public site icons                                                                                                         |
| `projects/shared` (shared lib) | ~10 components | toast.service, error-state, empty-state, confirm-delete-dialog, search-input, page-header, pdf-viewer, action-menu, badge |

## The four conversion patterns (proven in spike)

```html
<!-- 1. Static class -->
<i class="pi pi-truck"></i>
→ <svg lucideIcon="truck" class="size-4"></svg>

<!-- 2. Dynamic class binding -->
<i [class]="item.icon"></i>
→ <svg [lucideIcon]="item.icon" class="size-4"></svg>

<!-- 3. Mixed classes (preserve non-pi classes, swap text-Xrem → size-N) -->
<i class="pi pi-trash mb-4 text-6xl text-red-500"></i>
→ <svg lucideIcon="trash-2" class="mb-4 size-12 text-red-500"></svg>

<!-- 4. PrimeNG component icon input — STRUCTURAL change -->
<p-button label="Save" icon="pi pi-save" (onClick)="save()" />
→
<p-button label="Save" (onClick)="save()">
  <ng-template pTemplate="icon">
    <svg lucideIcon="save" class="size-4"></svg>
  </ng-template>
</p-button>
```

```typescript
// 5. TypeScript string literals
icon: "pi pi-truck"
→ icon: "truck"
```

For each component whose template now uses `lucideIcon`, add `LucideDynamicIcon` to the component's `imports: []` and `import { LucideDynamicIcon } from "@lucide/angular";`.

## Sizing rule

PrimeIcons uses `font-size` to size glyphs. Lucide ships SVGs that need explicit `width`/`height` (or Tailwind `size-N`). Conversion table:

| PrimeIcons (Tailwind) | Lucide (Tailwind) |
| --------------------- | ----------------- |
| `text-xs` (~12px)     | `size-3`          |
| `text-sm` (~14px)     | `size-3.5`        |
| `text-base` (~16px)   | `size-4`          |
| `text-lg` (~18px)     | `size-4.5`        |
| `text-xl` (~20px)     | `size-5`          |
| `text-2xl` (~24px)    | `size-6`          |
| `text-3xl` (~30px)    | `size-7`          |
| `text-4xl` (~36px)    | `size-9`          |
| `text-6xl` (~60px)    | `size-14`         |

When `nav-icon` / similar CSS classes use `font-size` to size, swap them to `width`+`height` in the CSS instead of Tailwind utilities. See [nav-menu.css](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/nav-menu/nav-menu.css) for the reference pattern.

## Icon name mapping (canonical, hand-curated)

Use this as the source of truth. **Don't auto-generate** — Lucide names don't 1:1 map from kebab-case PrimeIcon names (e.g. `pi-cog` ≠ `cog`, it's `settings`).

```json
{
  "pi-arrow-down": "arrow-down",
  "pi-arrow-left": "arrow-left",
  "pi-arrow-right": "arrow-right",
  "pi-arrow-up": "arrow-up",
  "pi-ban": "ban",
  "pi-bars": "menu",
  "pi-bell": "bell",
  "pi-bell-slash": "bell-off",
  "pi-bolt": "zap",
  "pi-book": "book",
  "pi-box": "package",
  "pi-briefcase": "briefcase",
  "pi-building": "building",
  "pi-calendar": "calendar",
  "pi-car": "car",
  "pi-chart-bar": "bar-chart-3",
  "pi-chart-line": "trending-up",
  "pi-check": "check",
  "pi-check-circle": "circle-check",
  "pi-check-square": "square-check",
  "pi-chevron-down": "chevron-down",
  "pi-chevron-left": "chevron-left",
  "pi-chevron-right": "chevron-right",
  "pi-chevron-up": "chevron-up",
  "pi-circle": "circle",
  "pi-clipboard": "clipboard",
  "pi-clock": "clock",
  "pi-cloud": "cloud",
  "pi-cog": "settings",
  "pi-comment": "message-circle",
  "pi-comments": "messages-square",
  "pi-copy": "copy",
  "pi-credit-card": "credit-card",
  "pi-directions": "navigation",
  "pi-dollar": "dollar-sign",
  "pi-download": "download",
  "pi-ellipsis-v": "ellipsis-vertical",
  "pi-envelope": "mail",
  "pi-exclamation-circle": "circle-alert",
  "pi-exclamation-triangle": "triangle-alert",
  "pi-external-link": "external-link",
  "pi-eye": "eye",
  "pi-eye-slash": "eye-off",
  "pi-file": "file",
  "pi-file-check": "file-check",
  "pi-file-edit": "file-pen-line",
  "pi-file-import": "file-input",
  "pi-file-invoice": "receipt",
  "pi-file-o": "file",
  "pi-file-pdf": "file-text",
  "pi-filter-slash": "filter-x",
  "pi-folder": "folder",
  "pi-folder-open": "folder-open",
  "pi-heart": "heart",
  "pi-history": "history",
  "pi-home": "house",
  "pi-id-card": "id-card",
  "pi-image": "image",
  "pi-inbox": "inbox",
  "pi-info-circle": "info",
  "pi-key": "key",
  "pi-link": "link",
  "pi-linkedin": "linkedin",
  "pi-list": "list",
  "pi-lock": "lock",
  "pi-map": "map",
  "pi-map-marker": "map-pin",
  "pi-microchip": "cpu",
  "pi-minus": "minus",
  "pi-minus-circle": "circle-minus",
  "pi-money-bill": "banknote",
  "pi-moon": "moon",
  "pi-pause-circle": "circle-pause",
  "pi-paw": "paw-print",
  "pi-pen-to-square": "square-pen",
  "pi-pencil": "pencil",
  "pi-play": "play",
  "pi-plus": "plus",
  "pi-plus-circle": "circle-plus",
  "pi-question-circle": "circle-help",
  "pi-refresh": "refresh-cw",
  "pi-route": "route",
  "pi-save": "save",
  "pi-search": "search",
  "pi-send": "send",
  "pi-shield": "shield",
  "pi-shopping-cart": "shopping-cart",
  "pi-sign-in": "log-in",
  "pi-sign-out": "log-out",
  "pi-sliders-h": "sliders-horizontal",
  "pi-snowflake": "snowflake",
  "pi-sort-down": "arrow-down-narrow-wide",
  "pi-sort-up": "arrow-up-narrow-wide",
  "pi-sparkles": "sparkles",
  "pi-star": "star",
  "pi-stop-circle": "circle-stop",
  "pi-sun": "sun",
  "pi-sync": "refresh-cw",
  "pi-table": "table",
  "pi-th-large": "layout-grid",
  "pi-times": "x",
  "pi-times-circle": "circle-x",
  "pi-trash": "trash-2",
  "pi-truck": "truck",
  "pi-undo": "undo-2",
  "pi-upload": "upload",
  "pi-user": "user",
  "pi-user-edit": "user-pen",
  "pi-user-plus": "user-plus",
  "pi-users": "users",
  "pi-wallet": "wallet",
  "pi-warehouse": "warehouse",
  "pi-wifi": "wifi",
  "pi-wifi-off": "wifi-off",
  "pi-wrench": "wrench"
}
```

### Special case: `pi-spin`

PrimeIcons ships a `pi-spin` utility that adds a CSS animation. Lucide doesn't ship animations. Port to a global CSS keyframe (one-time, in `projects/shared/src/lib/styles/...` or each portal's `styles.css`):

```css
@keyframes icon-spin {
  to {
    transform: rotate(360deg);
  }
}
.icon-spin {
  animation: icon-spin 1s linear infinite;
}
```

Then `pi pi-spin pi-spinner` → `<svg lucideIcon="loader-circle" class="icon-spin size-4">`.

## Suggested execution plan

### Stage 1 — Codemod script (~half a day)

Build `scripts/migrate-icons/migrate.ts` (Node + `ts-morph` for TS AST work):

1. Load `icon-map.json` (the table above). Fail loudly if any used `pi-X` is missing.
2. Walk `projects/{tms-portal,admin-portal,customer-portal,website,shared}/src/**/*.{html,ts}` — skip `lucide-icons.ts` and the already-migrated sidebar files.
3. Apply the 4 safe transforms above.
4. Per portal, collect every Lucide icon name that ended up in HTML/TS → emit `{portal}/shared/icons/lucide-icons.generated.ts` for human review/merge into the existing portal icon list.
5. For each unsafe site (PrimeNG `<p-button icon=>`, `<p-inputicon class=>`, `<button class="pi pi-X">` icon-as-button, `\`pi pi-${name}\``template strings), don't touch — append to`report.md`with`file:line:col` and the matched line.
6. For each component whose template now uses `lucideIcon`, add `LucideDynamicIcon` to the `imports: []` array via ts-morph, plus the `import` statement.
7. Run with `--dry-run` first; review the diff; then `--write`.

### Stage 2 — Manual review pass (~half a day)

Work through `report.md`:

- **PrimeNG `<p-button icon=>`** (~50+ sites) — convert to `pTemplate="icon"` template. The pattern is in `admin-portal/.../sidebar.html` lines 41-50.
- **`<p-inputicon class="pi pi-search">`** (~handful) — PrimeNG-specific; check PrimeNG 21 docs for the equivalent (likely `<p-inputicon><svg lucideIcon="search"></svg></p-inputicon>` or `iconClass`).
- **`<button class="pi pi-times ...">`** (icon-as-button, a few in `favorites-bar.html` already done) — split into `<button><svg></svg></button>`.
- **`\`pi pi-${name}\`` template strings** in `action-menu.ts`, `badge.ts`, `empty-state.ts` — these are shared primitives that accept an icon-name input. Their API needs to change from "PrimeIcons name suffix" to "Lucide name". Single-place change but it cascades to every consumer of those primitives. Check `feature-map.md` for usage.
- **`toast.service.ts`** hardcoded `pi pi-exclamation-triangle` (line 90) — swap to `lucide:triangle-alert` or whatever convention PrimeNG's `MessageService` icon field accepts (it's a class string, so likely needs the `pTemplate` approach in the toast template, not the service).

### Stage 3 — CSS sizing pass (~1-2 hours)

Grep for `text-Xrem` and `text-{xs,sm,base,lg,xl,...}` on icon elements and apply the conversion table above. Most cases the codemod can handle if the script is told which Tailwind text-classes commonly appear next to `pi pi-*`.

### Stage 4 — Cleanup (~30 min)

- Remove `primeicons/primeicons.css` import from 4 `styles.css` files: `tms-portal`, `admin-portal`, `customer-portal`, `website`
- Remove `"primeicons": "^7.0.0"` from `package.json`
- `bun install` + `bun run build:all` to verify no PrimeIcons class references remain
- Verify PrimeNG's internal icons (paginator chevrons, dropdown arrows, datepicker arrows) still work — PrimeNG 17+ uses inline SVG for these so removing PrimeIcons CSS shouldn't break them, but **smoke-test the data tables, paginator, and dropdowns** specifically.

### Stage 5 — Visual QA (~1-2 hours)

- TMS: loads list, load detail, sidebar, command palette, AI dispatch
- Admin: tenants list, blog post editor, plans
- Customer: shipments, public tracking page
- Website: footer, social icons

Look for: blank squares (icon name missing from registration), wrong-sized icons (sizing pass missed something), wrong-colored icons (Lucide inherits `currentColor` so should be fine, but verify).

## Total estimated time

**~1.5 days end-to-end** if done sequentially. Stage 1 is the high-value piece — it eliminates ~70% of mechanical work.

## Reference files (already migrated, study these for patterns)

- TMS sidebar: [nav-menu.html](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/nav-menu/nav-menu.html), [nav-menu.ts](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/nav-menu/nav-menu.ts), [nav-menu.css](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/nav-menu/nav-menu.css), [favorites-bar.html](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/sidebar/favorites-bar/favorites-bar.html), [sidebar-items.ts](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/sidebar/sidebar-items.ts)
- Admin sidebar (includes `<p-button>` template conversion): [sidebar.html](../src/Client/Logistics.Angular/projects/admin-portal/src/app/shared/layout/sidebar/sidebar.html), [sidebar.ts](../src/Client/Logistics.Angular/projects/admin-portal/src/app/shared/layout/sidebar/sidebar.ts), [sidebar-items.ts](../src/Client/Logistics.Angular/projects/admin-portal/src/app/shared/layout/sidebar/sidebar-items.ts)
- Icon registries: [shared base](../src/Client/Logistics.Angular/projects/shared/src/lib/icons/lucide-icons.ts), [TMS](../src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/icons/lucide-icons.ts), [admin](../src/Client/Logistics.Angular/projects/admin-portal/src/app/shared/icons/lucide-icons.ts)
- App configs: [tms](../src/Client/Logistics.Angular/projects/tms-portal/src/app/app.config.ts), [admin](../src/Client/Logistics.Angular/projects/admin-portal/src/app/app.config.ts)

## Pitfalls / lessons from the spike

1. **`LucideIcon` is a TS interface, not a directive.** The runtime directive is `LucideDynamicIcon`. Importing `LucideIcon` and putting it in `imports: []` produces the diagnostic "Value could not be determined statically." Always import `LucideDynamicIcon`.

2. **`provideLucideIcons(...)` accepts varargs**, registers under the `LUCIDE_ICONS` multi-provider token. Stacked spreads (`...BASE, ...PORTAL`) merge cleanly — don't worry about double-registering.

3. **The shared lib hasn't migrated yet.** `BASE_LUCIDE_ICONS` is forward-looking — it contains icons the shared lib _will need_ once its components migrate (toast, error-state, etc.). Today, those components still use PrimeIcons internally. Don't remove `primeicons` from `package.json` until the shared lib pass is done in Stage 2.

4. **PrimeNG's own internal chevrons/arrows** (paginator, dropdown, datepicker) are inline SVG since PrimeNG 17, NOT PrimeIcons. Removing `primeicons.css` shouldn't break them — but smoke-test in Stage 4.

5. **Tree-shaking works.** Each portal only ships the icons in its registered list — no full icon-set import. The bundle-size check in Stage 4 should show TMS dropping a few hundred KB once `primeicons.css` (~200KB raw) is gone.
