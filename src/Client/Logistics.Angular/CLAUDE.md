# Logistics Angular Workspace

For Angular code conventions (signals, control flow, host bindings, theme utilities), see `.claude/rules/frontend/angular-conventions.md` — auto-loaded on `.ts` / `.html` edits.

## Commands

```bash
bun install                # Install dependencies
bun run start:admin        # Admin Portal: https://localhost:7002
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
| `admin-portal`    | 7002 | `adm-` |
| `tms-portal`      | 7003 | `app-` |
| `customer-portal` | 7004 | `cp-`  |
| `website`         | 7005 | `web-` |
| `shared`          | N/A  | `ui-`  |

## Forms

**This repo is 100% Signal Forms** (`@angular/forms/signals`) — zero `ReactiveFormsModule`, zero
`formControlName`. Do not introduce either. See
`.claude/skills/signal-forms-migration/SKILL.md` for the full API.

Shared form building blocks live in `projects/shared/src/lib/ui/form/`, exported from
`@logistics/shared/ui`: `ui-form-field` plus the `*-field` controls (`ui-text-field`,
`ui-textarea-field`, `ui-select-field`, `ui-multiselect-field`, `ui-number-field`,
`ui-currency-field`, `ui-unit-field`, `ui-date-field`, `ui-checkbox-field`, `ui-toggle-field`,
`ui-password-field`, `ui-autocomplete-field`, `ui-search-field`, `ui-phone-field`) and the
composites `ui-address-form` / `ui-language-picker`.

Each `*-field` implements `FormValueControl` only — **never** a legacy value accessor — so
`[formField]` binds straight to it.

### Field wrapper

Always use `<ui-form-field>` instead of hand-building labels, hints, and error messages. It
**auto-resolves the field from the projected `[formField]`** and renders validation errors
reactively — no extra binding required:

```html
<form [formRoot]="form">
  <ui-form-field label="Email Address" for="email" [required]="true">
    <ui-text-field id="email" [formField]="form.email" type="email" />
  </ui-form-field>

  <ui-form-field label="Notes" for="notes" hint="Optional">
    <ui-textarea-field id="notes" [formField]="form.notes" [rows]="3" />
  </ui-form-field>

  <button type="submit" [disabled]="form().submitting()">Save</button>
</form>
```

```ts
protected readonly model = signal({ email: "", notes: "" });
protected readonly form = form(
  this.model,
  (p) => {
    required(p.email, { message: "Email address is required." });
    email(p.email, { message: "Enter a valid email address." });
  },
  {
    submission: {
      action: async () => {
        await this.api.invoke(saveThing, { body: this.model() });
        return undefined; // or ValidationError[] to attach server errors to fields
      },
    },
  },
);
```

Optional `hint="..."` for helper text. Pass `[field]="form.x"` only for the rare case where the
control is not a projected child (it overrides auto-resolution).

The class is `UiFormField` (selector `ui-form-field`). It is deliberately **not** named `FormField`,
because Angular's Signal Forms directive owns that name — `import { FormField } from "@angular/forms/signals"`.

### Reveal-on-submit (`ValidatedForm`)

Add the `ValidatedForm` directive to a form component's `imports`. It auto-applies to every
`<form [formRoot]>` in that component — no template attribute, no submit-handler changes. On an
invalid submit it scrolls to / focuses the first invalid control and announces the invalid-field
count via an `aria-live` region. It does **not** mark controls touched: Signal Forms' `submit()`
already marks the whole tree touched before checking validity, so inline `ui-form-field` errors
reveal themselves.

```ts
import { UiFormField, UiTextField, ValidatedForm } from "@logistics/shared/ui";
import { form, FormField, FormRoot, required, submit } from "@angular/forms/signals";
// ...
@Component({ imports: [FormRoot, FormField, UiFormField, UiTextField, ValidatedForm, /* ... */] })
```

`<form [formRoot]>` calls `submit()` itself when the form declares `submission` options. To submit
imperatively instead, call `submit(this.form, async () => { ... })`.

Do **not** disable the submit button with `[disabled]="form().invalid()"` — keep it clickable
(guard only on `form().submitting()`) so `ValidatedForm` can reveal what's missing. There is no
`ui-validation-summary`; inline field errors plus reveal-on-submit replace it.

## Theme files

- `projects/shared/src/styles/theme.css` — **canonical** shadcn token layer shared by tms / admin /
  customer (`--background`, `--card`, `--muted`, `--foreground`, `--border`, …). Light in `:root`,
  dark in `.dark-theme`. It also owns `color-scheme` (`light` on `:root`, `dark` on `.dark-theme`) —
  that declaration is what makes native scrollbars, form controls and the page canvas follow the
  theme. It is load-bearing: drop it and dark mode keeps light scrollbars.
- `projects/tms-portal/src/styles/variables.css` — the TMS raw ramp (`--bg-base`, `--text-primary`,
  `--primary-500`, …) that `theme.css` keys the canonical tokens to.

There is **no PrimeNG theme preset**. The four portals are styled entirely by `theme.css` + Tailwind;
`primeng-preset.ts` and `providePrimeNG()` were deleted in the S13 migration step.

For the `bg-elevated` / `bg-subtle` / `border-default` / `text-muted` rule (and the no-hardcoded-colors rule), see `angular-conventions.md`.

## UI library

**spartan/ui** (Helm, vendored in-repo) — see `.claude/rules/frontend/angular-conventions.md` for the
full `ui-*` catalogue. PrimeNG is gone: no dependency, no import, no `p-*` markup. Reintroducing one is
blocked by the ESLint `no-restricted-imports` rule in `eslint.config.js`, which fails lint on any
`primeng`/`primeicons`/`@primeuix/*` import.

```bash
bun run ng test shared          # the shared-library specs
bun run check:spartan-tokens    # fails on bare spartan-* CSS classes in primitives (they'd render unstyled)
```

`/ui-lab` is a lazy dev route in tms-portal that renders every `ui-*` component in light and dark.
