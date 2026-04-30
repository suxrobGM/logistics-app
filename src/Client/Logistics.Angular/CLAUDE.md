# Logistics Angular Workspace

See the `angular-workspace` skill for detailed commands and patterns.

## Quick Reference

```bash
bun install                # Install dependencies
bun run start:tms          # TMS Portal: https://localhost:7003
bun run start:customer     # Customer Portal: https://localhost:7004
bun run start:website      # Website: https://localhost:7005
bun run build:all          # Build all projects
bun run gen:api            # Regenerate API client from swagger.json
bun run gen:api:live       # Fetch latest spec from running API + regenerate
bun run lint               # Lint code
```

## Projects

| Project           | Port | Prefix |
| ----------------- | ---- | ------ |
| `tms-portal`      | 7003 | `app-` |
| `customer-portal` | 7004 | `cp-`  |
| `website`         | 7005 | `web-` |
| `shared`          | N/A  | `ui-`  |

## Key Patterns

- Standalone components with separate HTML templates
- Signals + `@ngrx/signals` for state management
- `input()`/`output()` functions (not decorators)
- Native control flow (`@if`, `@for`)

## Form Fields

**IMPORTANT:** Always use the `<ui-form-field>` component for form fields instead of manually creating labels, hints, and error messages.

```typescript
import { FormField } from "@logistics/shared";
```

### Usage

```html
<!-- Basic usage -->
<ui-form-field label="Name" for="name" [required]="true">
  <input pInputText id="name" formControlName="name" />
</ui-form-field>

<!-- With hint text -->
<ui-form-field label="Email" for="email" hint="We'll never share your email">
  <input pInputText id="email" formControlName="email" type="email" />
</ui-form-field>

<!-- With validation (shows errors automatically) -->
<ui-form-field label="Password" for="password" [required]="true" [control]="form.controls.password">
  <input pInputText id="password" formControlName="password" type="password" />
</ui-form-field>
```

## Theme System (TMS Portal)

The TMS Portal uses a dual theme system with CSS custom properties and PrimeNG preset.

### Theme Files

- `projects/tms-portal/src/styles/variables.css` - CSS custom properties for light/dark themes
- `projects/tms-portal/src/app/core/theme/primeng-preset.ts` - PrimeNG component customizations

### Theme-Aware Tailwind Utilities

**IMPORTANT:** Never use hardcoded light/dark classes like `bg-white dark:bg-surface-900`. Instead, use these theme-aware utilities that automatically adapt to the current theme

### Examples

```html
<!-- WRONG - hardcoded colors break dark theme -->
<div class="dark:bg-surface-900 border-gray-200 bg-white dark:border-gray-700">
  <!-- CORRECT - theme-aware utilities -->
  <div class="bg-elevated border-default rounded-lg p-4">
    <!-- Cards and panels -->
    <div class="border-default bg-elevated rounded-lg border p-4">
      <!-- Info boxes -->
      <div class="border-default bg-subtle rounded-lg border p-4"></div>
    </div>
  </div>
</div>
```
