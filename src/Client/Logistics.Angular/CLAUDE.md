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

**`projects/shared/src/styles/theme.css` is the whole design system** for tms / admin / customer — one
palette (`:root` light, `.dark-theme` dark), the canonical shadcn tokens keyed to it, the component
knobs (`--ui-btn-*`, `--ui-card-*`, `--ui-table-th-*`, `--ui-radius-*`), and the base element layer.
Each portal's `styles.css` is ~4 lines: `@import "tailwindcss"` then `@import theme.css`.

A knob earns its place by having MORE THAN ONE VALUE. `--ui-badge-*` held the badge's colours and
metrics for the Nora/Aura preset fork; with the presets gone it resolved one way, so the badge spells
named utilities directly (`badges/badge/badge-variants.ts`, whose `TONE_CLASSES` `ui-count-badge`
shares). Don't reintroduce a single-valued knob.

The three portals are theme-identical by construction. Only `projects/tms-portal/src/styles/chrome.css`
sits on top, and only for what is genuinely TMS-only: its sidebar gradients and its animations.

Two things in `theme.css` are load-bearing and look droppable:

- **`color-scheme`** (`light` on `:root`, `dark` on `.dark-theme`) — what makes native scrollbars, form
  controls and the page canvas follow the theme. Each portal's `index.html` must also carry
  `<meta name="color-scheme" content="dark light">`; `only light` overrides it and dark mode half-works.
- **The canonical tokens alias the palette and are NOT re-declared under `.dark-theme`.** That works only
  because `.dark-theme` lands on `<html>`, the same element `:root` selects. Move it to `<body>` and
  every alias freezes at its light value.

The website is deliberately outside this: it keeps its own editorial palette and has no dark mode, so it
re-pins Tailwind's radius scale and its own fonts after importing `theme.css`.

**Never hardcode a colour** (`bg-white`, `text-gray-600`, `bg-green-100`) — it does not follow the theme,
survives every build/test/lint, and then renders a white sidebar on a dark page. That is exactly how
admin/customer dark mode broke. Use a semantic token (`bg-card`, `bg-background`, `bg-sidebar`,
`text-muted-foreground`, `border-border`, `text-success`, `bg-success/15`). `bun run check:theme-tokens`
enforces this on admin + customer.

There is **no PrimeNG theme preset**; the portals are styled entirely by `theme.css` + Tailwind.

## UI library

**spartan/ui** (Helm, vendored in-repo) — see `.claude/rules/frontend/angular-conventions.md` for the
full `ui-*` catalogue. PrimeNG is gone: no dependency, no import, no `p-*` markup. Reintroducing one is
blocked by the ESLint `no-restricted-imports` rule in `eslint.config.js`, which fails lint on any
`primeng`/`primeicons`/`@primeuix/*` import.

`ui-dialog` / `ui-confirm-dialog` deliberately keep a hand-rolled Escape handler instead of brain's
`disableClose`. Brain gates Escape and backdrop-click on that one flag, and its `backdropClick()` is
otherwise ungated — binding it would give every dialog click-outside-to-discard. There is no "escape
closes, backdrop does not" option. Do not "simplify" this.

```bash
bun run ng test shared          # the shared-library specs
bun run check                   # every gate in tools/checks/ — this is what CI runs
bun run check --self-test       # prove each gate still fires
bun run check:spartan-tokens    # bare spartan-* classes in primitives (they'd render unstyled)
bun run check:theme-tokens      # hardcoded palette colours in admin/customer
```

`bun run check` DISCOVERS its gates: any file in `tools/checks/` exporting a `check` runs. To add one,
declare it with `defineCheck({ dirs, ext, find, cases })` from `tools/lib/check.mjs` — no script and no
CI step to add.

`/ui-lab` is a lazy dev route in tms-portal that renders every `ui-*` component in light and dark.
