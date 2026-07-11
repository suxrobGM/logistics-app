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

- The UI library is **spartan/ui** — Helm components vendored in-repo under `projects/shared/src/lib/ui/primitives/` on top of `@spartan-ng/brain`. **NOT PrimeNG.**
- **PrimeNG is being removed — never add a new `p-*` component or a `primeng/*` import.** Existing ones are being swept out.
- **Prefer the shared `ui-*` components from `@logistics/shared/ui` over hand-rolled Tailwind.** Never reach for a raw Helm primitive in feature code — the `ui-*` components are the public surface. What exists today:
  - **Forms**: `ui-form-field` (label/hint/error wrapper — auto-resolves the projected `[formField]`), `ui-text-field`, `ui-textarea-field`, `ui-select-field`, `ui-multiselect-field`, `ui-number-field`, `ui-currency-field`, `ui-unit-field`, `ui-date-field`, `ui-checkbox-field`, `ui-toggle-field`, `ui-password-field`, `ui-autocomplete-field`, `ui-search-field`, `ui-phone-field`, `ui-address-form`, `ui-language-picker`, plus the `ValidatedForm` directive (matches `form[formRoot]`)
  - **Data**: `ui-data-table` with `<th uiSortHeader="Field">`
  - **Content**: `ui-icon`, `ui-alert`, `ui-badge`, `ui-status-badge`, `ui-typography`, `ui-theme-toggle`
  - **Layout**: `ui-divider`, `ui-container`, `ui-grid`, `ui-stack`, `ui-surface`, `ui-toolbar`, `ui-page-header`, `ui-dashboard-card`, `ui-tabs` (+ `ui-tab-list` / `ui-tab` / `ui-tab-panels` / `ui-tab-panel`), `ui-accordion` (+ `-panel` / `-header` / `-content`), `ui-stepper` (+ `ui-step-list` / `ui-step` / `ui-step-panels` / `ui-step-panel` and the `*uiStepContent` template), `ui-drawer`
  - **Feedback**: `ui-empty-state`, `ui-error-state`, `ui-loading-skeleton`, `ui-data-container`, `ui-date-range-picker`, `ui-popover`
  - **Menus**: `ui-menu` — a popup menu driven by a template ref: `<ui-menu #menu [items]="items()" />` plus `(click)="menu.toggle($event)"` on any trigger. Items are `UiMenuItem` (`icon` is a typed `IconName`, `variant: "destructive"` for danger rows). **Never import `MenuItem` from `primeng/api`.**
  - **Timeline**: `ui-timeline` with `*uiTimelineContent` (and optional `*uiTimelineMarker`) templates
- `ui-chart` is still **landing during the migration** — check `projects/shared/src/lib/ui/` for what exists before hand-rolling one
- **Never hide an `<ng-icon>` with a Tailwind display utility** (`hidden`, `inline`, …). `NgIcon` ships an unlayered `:host { display: inline-block }` component style, and unlayered CSS beats every `@layer` — including `@layer utilities`, where all Tailwind utilities live. The class lands in the DOM, the rule lands in the stylesheet, and the icon stays visible anyway. Rotate it (`rotate-180`) or wrap it in a `<span>` you hide instead.
- **Icons**: `<ui-icon name="..."/>` only. Never `<i class="pi pi-*">`, never a raw `<ng-icon>` in feature code
- **Toasts / confirms**: `ToastService` from `@logistics/shared` only. Never inject `MessageService` / `ConfirmationService`

## HTTP Caching

- In-memory cache interceptor (`cacheInterceptor`) caches GET requests based on rules in `projects/shared/src/lib/api/cache.config.ts`
- Rules are evaluated in order — first match wins, catch-all default is 2 min TTL
- **Set `ttl: 0`** for endpoints that receive real-time updates via SignalR (e.g., dispatch, messages)
- Cache is auto-invalidated on POST/PUT/PATCH/DELETE to the same base path
- When adding new real-time features, always add a no-cache rule BEFORE the catch-all
