---
paths:
  - "src/Client/Logistics.Angular/**/*.ts"
  - "src/Client/Logistics.Angular/**/*.html"
---

# Angular Code Conventions

## Components

- Standalone components (don't add `standalone: true` — it's the default in Angular 20+)
- Separate template files (`templateUrl`), no inline templates/styles
- Files: `{name}.ts`, `{name}.html` (not `{name}.component.ts`)
- Prefixes: tms=`app-`, customer=`cp-`, website=`web-`, shared=`ui-`

## Signals & Reactivity

- `signal()` for local state, `computed()` for derived state
- `input()` / `output()` functions — NOT `@Input`/`@Output` decorators
- `@ngrx/signals` stores for complex state

## DI & Access Modifiers

- `inject()` function, not constructor injection
- `private readonly` for services, `protected readonly` for template-used stores
- `protected` for template-bound properties/methods, `private` for internal

## Templates

- Native control flow: `@if`, `@for`, `@switch` — NOT `*ngIf`, `*ngFor`
- Use `@empty` block in `@for` for empty states

## Imports

- Shared: `import { X } from "@logistics/shared";`
- API models: `import type { XDto } from "@logistics/shared/api/models";`
- App-internal: `import { X } from "@/core/services";`

## Host Bindings

- Use `host` property in `@Component` decorator — NOT `@HostListener` / `@HostBinding` decorators

```typescript
// Good
@Component({
  host: {
    '(document:keydown)': 'onKeydown($event)',
    '[class.active]': 'isActive()',
  }
})

// Bad — deprecated pattern
@HostListener('document:keydown', ['$event'])
onKeydown(event: KeyboardEvent) {}
```

## Animations

- `provideAnimationsAsync()` is deprecated in Angular 21+ — do NOT add it to app.config.ts
- Angular 21 enables animations by default, no provider needed

## Styling

- Tailwind CSS utilities preferred, avoid custom CSS unless necessary
- **Never use hardcoded color values** (e.g., `bg-yellow-50`, `text-yellow-700`, `border-yellow-200`) — use the theme-aware utilities (`bg-subtle`, `bg-elevated`, `border-default`, `text-muted`) or the shadcn tokens (`bg-background`, `bg-card`, `bg-muted`, `text-foreground`, `text-muted-foreground`, `border-border`). Tokens live in `projects/shared/src/styles/theme.css`

## UI components

- The UI library is **spartan/ui** — Helm components vendored in-repo under `projects/shared/src/lib/ui/primitives/` on top of `@spartan-ng/brain`. **PrimeNG is GONE** (removed in full; no dependency, no import, no `p-*` markup anywhere). Never reintroduce a `p-*` component or a `primeng/*` import — CI lint fails on it (`no-restricted-imports` in `eslint.config.js`).
- **Always prefer a shared `ui-*` component from `@logistics/shared/ui` over hand-rolled Tailwind.** The Helm primitives under `ui/primitives/` are an implementation detail and are deliberately **not** re-exported — never reach for a raw `hlm*` directive in feature code. The full public catalogue:
  - **Action**: `ui-button`, `ui-toggle-group`, `ui-theme-toggle`
  - **Forms**: `ui-form-field` (label/hint/error wrapper — auto-resolves the projected `[formField]`), `ui-text-field`, `ui-textarea-field`, `ui-select-field`, `ui-multiselect-field`, `ui-number-field`, `ui-currency-field`, `ui-unit-field`, `ui-date-field`, `ui-date-range-picker`, `ui-checkbox-field`, `ui-toggle-field`, `ui-password-field`, `ui-autocomplete-field`, `ui-search-field`, `ui-phone-field`, `ui-editor` (lazy Quill), `ui-file-upload`, the composites `ui-address-form` / `ui-language-picker`, and the `ValidatedForm` directive (auto-applies to `form[formRoot]`)
  - **Table**: `ui-data-table` with `<th uiSortHeader="Field">`, `ui-table-paginator`, `<tr uiSelectableRow>`, `[uiRowToggler]`, `ui-table-checkbox`, `ui-table-header-checkbox`
  - **Badges**: `ui-badge`, `ui-status-badge`, `ui-count-badge`, `ui-overlay-badge`
  - **Display**: `ui-typography`, `ui-avatar`, `ui-timeline`, `ui-chart`, `ui-money-with-tax`, `ui-pdf-viewer`, `ui-alert`
  - **Overlay**: `ui-dialog`, `ui-confirm-dialog`, `ui-confirm-delete-dialog`, `ui-popover`, `[uiTooltip]`, `ui-drawer`, `ui-menu`, `ui-lightbox`, `ui-toaster`, `ui-cookie-banner`
  - **Status**: `ui-spinner`, `ui-skeleton`, `ui-loading-skeleton`, `ui-progress`, `ui-empty-state`, `ui-error-state`, `ui-data-container`
  - **Layout**: `ui-stack`, `ui-surface`, `ui-container`, `ui-grid`, `ui-divider`, `ui-toolbar` (+ responsive helpers)
  - **Containers**: `ui-card`, `ui-dashboard-card`, `ui-feature-row`, `ui-page-header`
  - **Disclosure**: `ui-tabs` (+ `ui-tab-list` / `ui-tab` / `ui-tab-panels` / `ui-tab-panel`), `ui-accordion` (+ `-panel` / `-header` / `-content`), `ui-stepper` (+ `ui-step-list` / `ui-step` / `ui-step-panels` / `ui-step-panel` and the `*uiStepContent` template), `ui-collapsible`
  - **Menus**: `ui-menu` — a popup menu driven by a template ref: `<ui-menu #menu [items]="items()" />` plus `(click)="menu.toggle($event)"` on any trigger. Items are `UiMenuItem` (`icon` is a typed `IconName`; `variant: "destructive"` for danger rows; `visible: false` **removes** an item)
  - **Timeline**: `ui-timeline` with `*uiTimelineContent` (and optional `*uiTimelineMarker`) templates
- Browse `/ui-lab` (lazy dev route in tms-portal) to see every component rendered in light and dark before building a new one.
- **Never hide an `<ng-icon>` with a Tailwind display utility** (`hidden`, `inline`, …). `NgIcon` ships an unlayered `:host { display: inline-block }` component style, and unlayered CSS beats every `@layer` — including `@layer utilities`, where all Tailwind utilities live. The class lands in the DOM, the rule lands in the stylesheet, and the icon stays visible anyway. Rotate it (`rotate-180`) or wrap it in a `<span>` you hide instead.
- **Icons**: `<ui-icon name="..."/>` only — never a raw `<ng-icon>` in feature code. `name` is the typed `IconName` union (a key of `UI_ICONS` in `projects/shared/src/lib/ui/icons/icons.ts`); an unknown name is a **compile error**. To add a glyph, import its `@ng-icons/lucide` export and add one entry to `UI_ICONS`.
- **Toasts / confirms**: `ToastService` from `@logistics/shared` only.

## HTTP Caching

- In-memory cache interceptor (`cacheInterceptor`) caches GET requests based on rules in `projects/shared/src/lib/api/cache.config.ts`
- Rules are evaluated in order — first match wins, catch-all default is 2 min TTL
- **Set `ttl: 0`** for endpoints that receive real-time updates via SignalR (e.g., dispatch, messages)
- Cache is auto-invalidated on POST/PUT/PATCH/DELETE to the same base path
- When adding new real-time features, always add a no-cache rule BEFORE the catch-all
