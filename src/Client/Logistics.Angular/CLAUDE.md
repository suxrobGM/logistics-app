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

## Form Fields

Always use `<ui-form-field>` from `@logistics/shared` instead of manually building labels, hints, and error messages. Pass the form control to render validation automatically.

```html
<ui-form-field label="Password" for="password" [required]="true" [control]="form.controls.password">
  <input pInputText id="password" formControlName="password" type="password" />
</ui-form-field>
```

Optional `hint="..."` for helper text.

## Theme files (TMS Portal)

- `projects/tms-portal/src/styles/variables.css` — CSS custom properties for light/dark themes
- `projects/tms-portal/src/app/core/theme/primeng-preset.ts` — PrimeNG component customizations

For the `bg-elevated` / `bg-subtle` / `border-default` / `text-muted` rule (and the no-hardcoded-colors rule), see `angular-conventions.md`.
