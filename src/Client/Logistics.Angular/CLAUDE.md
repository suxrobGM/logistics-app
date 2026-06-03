# Logistics Angular Workspace

For Angular code conventions (signals, control flow, host bindings, theme utilities), see `.claude/rules/frontend/angular-conventions.md` — auto-loaded on `.ts` / `.html` edits.

## Commands

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

## Forms

Shared form building blocks live in `projects/shared/src/lib/components/form/` (exported from
`@logistics/shared/components`). The `*-field` components (`ui-form-field`, `ui-currency-field`,
`ui-unit-field`, `ui-phone-field`, `ui-search-field`) replaced the old `*-input` names.

### Field wrapper

Always use `<ui-form-field>` instead of hand-building labels, hints, and error messages. It
**auto-resolves the control from the projected `formControlName`** and renders validation errors
reactively — no `[control]` binding required:

```html
<ui-form-field label="Password" for="password" [required]="true">
  <input pInputText id="password" formControlName="password" type="password" />
</ui-form-field>
```

Optional `hint="..."` for helper text. Pass `[control]="form.controls.x"` only for the rare case
where the control is not a projected child (it overrides auto-resolution).

### Reveal-on-submit (`ValidatedForm`)

Add the `ValidatedForm` directive to a form component's `imports`. It auto-applies to every
`<form [formGroup]>` in that component — no template attribute, no submit-handler changes. On an
invalid submit it marks all controls touched (so inline `ui-form-field` errors render), scrolls
to / focuses the first invalid control, and announces the error count via an `aria-live` region.

```ts
import { FormField, ValidatedForm } from "@logistics/shared/components";
// ...
@Component({ imports: [ReactiveFormsModule, FormField, ValidatedForm, /* ... */] })
```

Do **not** disable the submit button with `[disabled]="form.invalid"` — keep it clickable
(guard only on `isLoading()`) so `ValidatedForm` can reveal what's missing. There is no
`ui-validation-summary`; inline field errors plus reveal-on-submit replace it.

## Theme files (TMS Portal)

- `projects/tms-portal/src/styles/variables.css` — CSS custom properties for light/dark themes
- `projects/tms-portal/src/app/core/theme/primeng-preset.ts` — PrimeNG component customizations

For the `bg-elevated` / `bg-subtle` / `border-default` / `text-muted` rule (and the no-hardcoded-colors rule), see `angular-conventions.md`.
