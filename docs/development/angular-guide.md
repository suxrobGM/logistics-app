# Angular Development Guide

Patterns and conventions for the Angular workspace (Angular 22, zoneless, Signal Forms, spartan/ui).

## Workspace Structure

The Angular frontend uses a monorepo workspace with multiple projects:

```text
src/Client/Logistics.Angular/
├── angular.json                    # Workspace configuration
├── package.json                    # Shared dependencies
├── tsconfig.json                   # Base TypeScript config
├── ng-openapi-gen.json             # API client generation
├── projects/
│   ├── shared/                     # @logistics/shared library
│   │   ├── src/lib/
│   │   │   ├── api/                # Generated API client
│   │   │   ├── errors/             # Error handling
│   │   │   ├── services/           # Shared services (toast, cache)
│   │   │   ├── interceptors/       # HTTP interceptors
│   │   │   └── ui/                 # Design system (@logistics/shared/ui)
│   │   │       ├── primitives/     # Vendored spartan/ui Helm components
│   │   │       ├── form/           # ui-form-field + the ui-*-field controls
│   │   │       ├── table/          # ui-data-table
│   │   │       ├── layout/         # ui-stack, ui-grid, ui-page-header, ...
│   │   │       ├── containers/     # ui-card, ui-dashboard-card, ...
│   │   │       ├── badges/         # ui-badge, ui-status-badge, ...
│   │   │       ├── display/        # ui-alert, ui-typography, ui-avatar, ...
│   │   │       ├── overlay/        # ui-dialog, ui-popover, ui-toaster, ...
│   │   │       ├── status/         # ui-empty-state, ui-error-state, ...
│   │   │       ├── disclosure/     # ui-tabs, ui-accordion, ui-stepper, ...
│   │   │       └── icons/          # UI_ICONS record (icons.ts) + ui-icon
│   │   └── ng-package.json
│   ├── admin-portal/               # Admin Portal (super admin)
│   │   └── src/app/
│   │       ├── core/               # Admin auth
│   │       └── pages/              # Tenant, subscription management
│   ├── tms-portal/                 # TMS Portal (dispatchers/managers)
│   │   └── src/app/
│   │       ├── core/               # App-specific auth, services
│   │       ├── shared/             # TMS-specific components
│   │       └── pages/              # Feature pages
│   ├── customer-portal/            # Customer Portal (self-service)
│   │   └── src/app/
│   │       ├── core/               # Customer auth
│   │       ├── shared/             # Customer components
│   │       └── pages/              # Customer features
│   └── website/                    # Marketing Website (SSR)
│       └── src/app/
│           ├── layout/             # Header, footer
│           └── pages/              # Home, about, blog
└── scripts/
```

## Projects

| Project                    | Prefix | Port | Description                           |
| -------------------------- | ------ | ---- | ------------------------------------- |
| @logistics/shared          | `ui-`  | N/A  | Shared library (API, services, pipes) |
| @logistics/admin-portal    | `adm-` | 7002 | Super admin management                |
| @logistics/tms-portal      | `app-` | 7003 | Internal TMS for dispatchers          |
| @logistics/customer-portal | `cp-`  | 7004 | Customer self-service portal          |
| @logistics/website         | `web-` | 7005 | Marketing website (SSR)               |

## TMS Portal Structure

```text
projects/tms-portal/src/app/
├── core/                    # Singleton services
│   ├── auth/                # Authentication (OIDC)
│   ├── interceptors/        # HTTP interceptors
│   └── services/            # Core services (messaging, etc.)
├── shared/                  # Shared components
│   ├── components/          # Reusable UI components
│   └── pipes/               # Custom pipes
├── pages/                   # Feature pages
│   ├── dashboard/
│   ├── loads/
│   ├── customers/
│   ├── messages/            # Real-time messaging
│   ├── inspections/         # Vehicle condition reports
│   └── ...
├── app.component.ts
├── app.config.ts
└── app.routes.ts
```

## Key Patterns

### Standalone Components Only

No NgModules. All components are standalone:

```typescript
@Component({
  selector: "app-load-list",
  imports: [CommonModule, TableModule, ButtonModule],
  templateUrl: "./load-list.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadListComponent {}
```

### Signal-Based State

Use signals for reactive state:

```typescript
@Component({ ... })
export class LoadListComponent {
  private loadService = inject(LoadService);

  // State signals
  loads = signal<Load[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // Computed values
  activeLoads = computed(() =>
    this.loads().filter(l => l.status === 'Active')
  );

  async loadData() {
    this.loading.set(true);
    try {
      const result = await this.loadService.getLoads();
      this.loads.set(result.data);
    } catch (e) {
      this.error.set('Failed to load data');
    } finally {
      this.loading.set(false);
    }
  }
}
```

### Input/Output Functions

Use `input()` and `output()` instead of decorators:

```typescript
@Component({ ... })
export class LoadCardComponent {
  // Inputs
  load = input.required<Load>();
  showActions = input(true);

  // Outputs
  edit = output<Load>();
  delete = output<string>();

  onEdit() {
    this.edit.emit(this.load());
  }

  onDelete() {
    this.delete.emit(this.load().id);
  }
}
```

### Native Control Flow

Use `@if`, `@for`, `@switch` instead of structural directives:

```html
<!-- Good -->
@if (loading()) {
<ui-loading-skeleton />
} @else if (error()) {
<ui-error-state [message]="error()" />
} @else { @for (load of loads(); track load.id) {
<app-load-card [load]="load" />
} @empty {
<p>No loads found</p>
} }

<!-- Avoid -->
<ui-loading-skeleton *ngIf="loading"></ui-loading-skeleton>
<div *ngFor="let load of loads">...</div>
```

### Inject Function

Use `inject()` instead of constructor injection:

```typescript
@Component({ ... })
export class LoadListComponent {
  private loadService = inject(LoadService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  // No constructor needed for DI
}
```

### OnPush Change Detection

Always use OnPush:

```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

### Signal Forms

The workspace is **100% Signal Forms** (`@angular/forms/signals`). There is no `ReactiveFormsModule`
and no `formControlName` anywhere — do not introduce either. A form is a model `signal()` plus a
`form()` schema:

```typescript
@Component({
  imports: [FormRoot, FormField, UiFormField, UiTextField, UiNumberField, ValidatedForm],
  // ...
})
export class LoadFormComponent {
  private readonly api = inject(Api);

  protected readonly model = signal({ customerId: "", origin: "", destination: "", weight: 0 });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.customerId, { message: "Customer is required." });
      required(p.origin, { message: "Origin is required." });
      maxLength(p.origin, 200, { message: "Origin must be 200 characters or fewer." });
      required(p.destination, { message: "Destination is required." });
      maxLength(p.destination, 200, { message: "Destination must be 200 characters or fewer." });
      min(p.weight, 0, { message: "Weight cannot be negative." });
    },
    {
      submission: {
        action: async () => {
          await this.api.invoke(createLoad, { body: this.model() });
          return undefined; // or ValidationError[] to attach server errors to fields
        },
      },
    },
  );
}
```

```html
<form [formRoot]="form">
  <ui-form-field label="Origin" for="origin" [required]="true">
    <ui-text-field id="origin" [formField]="form.origin" />
  </ui-form-field>

  <button type="submit" [disabled]="form().submitting()">Save</button>
</form>
```

`<form [formRoot]>` runs `submission.action` on submit: it marks the whole tree touched _first_
(so inline errors reveal themselves), skips the action while invalid, and drives
`form().submitting()`. No `markAllAsTouched()`, no `if (form.invalid) return` guard, no
`(ngSubmit)`. Never gate the submit button on `form().invalid()` — keep it clickable so the
`ValidatedForm` directive can focus the first invalid control.

Full API and migration recipes: `.claude/skills/signal-forms-migration/SKILL.md`.

## API Client

### Regenerating Client

After API changes, regenerate the TypeScript client:

```bash
bun run gen:api
```

This reads the OpenAPI spec from the running API and generates typed services in `projects/shared/src/lib/api/`.

### Using API Services

Import API services from the shared library:

```typescript
import { LoadsApiService } from '@logistics/shared/api';

@Component({ ... })
export class LoadListComponent {
  private api = inject(LoadsApiService);

  async loadData() {
    const response = await firstValueFrom(
      this.api.getLoads({ page: 1, pageSize: 20 })
    );

    if (response.isSuccess) {
      this.loads.set(response.data.items);
    }
  }
}
```

### Shared Library Imports

```typescript
// API services
import { CustomersApiService, LoadsApiService } from "@logistics/shared/api";
// Error handling
import { ErrorHandlerService } from "@logistics/shared/errors";
// Common services
import { HttpCacheService, ToastService } from "@logistics/shared/services";
```

## Common Commands

```bash
# Development servers
bun run start:admin       # Admin Portal on https://localhost:7002
bun run start:tms         # TMS Portal on https://localhost:7003
bun run start:customer    # Customer Portal on https://localhost:7004
bun run start:website     # Website on http://localhost:7005

# Build
bun run build:shared      # Build shared library
bun run build:tms         # Build TMS Portal
bun run build:customer    # Build Customer Portal
bun run build:all         # Build all projects

# Linting & formatting
bun run lint
bun run format

# Generate API client (outputs to shared library)
bun run gen:api
```

## UI Components

The UI library is **spartan/ui**: Helm components vendored in-repo under
`projects/shared/src/lib/ui/primitives/` on top of `@spartan-ng/brain`.

**PrimeNG is gone** — fully removed (no dependency, no import, no `p-*` markup, no theme preset).
Never reintroduce a `p-*` component or a `primeng/*` import: the ESLint `no-restricted-imports` rule
in `eslint.config.js` fails lint on any `primeng`/`primeicons`/`@primeuix/*` import. Browse `/ui-lab`
(a lazy dev route in tms-portal) to see every `ui-*` component rendered in light and dark before
hand-rolling anything new.

Feature code does not touch the Helm primitives directly. It uses the shared `ui-*` components from
`@logistics/shared/ui`, which live in `projects/shared/src/lib/ui/`:

```typescript
import {
  Alert, // ui-alert, ui-badge, ui-status-badge, ui-typography, ui-theme-toggle
  EmptyState, // ui-empty-state, ui-error-state, ui-loading-skeleton, ui-data-container
  Icon, // ui-icon — the only way to render an icon
  Stack, // ui-stack, ui-grid, ui-container, ui-surface, ui-toolbar, ui-page-header
  UiDataTable, // ui-data-table, with <th uiSortHeader="Field">
  UiFormField, // label / hint / error wrapper
  UiTextField, // ...and select, multiselect, number, currency, unit, date,
} from "@logistics/shared/ui";
```

| Area       | Components                                                                                                                                                                                                                                                                                                                                                             |
| ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Forms      | `ui-form-field`, `ui-text-field`, `ui-textarea-field`, `ui-select-field`, `ui-multiselect-field`, `ui-number-field`, `ui-currency-field`, `ui-unit-field`, `ui-date-field`, `ui-date-range-picker`, `ui-checkbox-field`, `ui-toggle-field`, `ui-password-field`, `ui-autocomplete-field`, `ui-search-field`, `ui-phone-field`, `ui-address-form`, `ui-language-picker` |
| Table      | `ui-data-table` (+ `<th uiSortHeader="Field">`), `ui-table-paginator`                                                                                                                                                                                                                                                                                                  |
| Action     | `ui-button`, `ui-toggle-group`, `ui-theme-toggle`                                                                                                                                                                                                                                                                                                                      |
| Badges     | `ui-badge`, `ui-status-badge`, `ui-count-badge`, `ui-overlay-badge`                                                                                                                                                                                                                                                                                                    |
| Display    | `ui-typography`, `ui-avatar`, `ui-timeline`, `ui-chart`, `ui-money-with-tax`, `ui-pdf-viewer`, `ui-alert`                                                                                                                                                                                                                                                              |
| Overlay    | `ui-dialog`, `ui-confirm-dialog`, `ui-confirm-delete-dialog`, `ui-popover`, `[uiTooltip]`, `ui-drawer`, `ui-menu`, `ui-lightbox`, `ui-toaster`, `ui-cookie-banner`                                                                                                                                                                                                     |
| Status     | `ui-spinner`, `ui-skeleton`, `ui-loading-skeleton`, `ui-progress`, `ui-empty-state`, `ui-error-state`, `ui-data-container`                                                                                                                                                                                                                                             |
| Layout     | `ui-divider`, `ui-container`, `ui-grid`, `ui-stack`, `ui-surface`, `ui-toolbar`                                                                                                                                                                                                                                                                                        |
| Containers | `ui-card`, `ui-dashboard-card`, `ui-feature-row`, `ui-page-header`                                                                                                                                                                                                                                                                                                     |
| Disclosure | `ui-tabs`, `ui-accordion`, `ui-stepper`, `ui-collapsible`                                                                                                                                                                                                                                                                                                              |

Check `projects/shared/src/lib/ui/` for the full set before hand-rolling a new component.

Icons are `<ui-icon name="..."/>` only — never a raw `<ng-icon>` in feature code. `name` is the typed
`IconName` union (a key of the `UI_ICONS` record in `projects/shared/src/lib/ui/icons/icons.ts`); an
unknown static name is a compile error. To add a glyph, import its `@ng-icons/lucide` export and add
one entry to `UI_ICONS`. Toasts and confirmation dialogs go through `ToastService` from
`@logistics/shared` — never inject `MessageService` or `ConfirmationService`.

## Routing

Routes are defined in `app.routes.ts`:

```typescript
export const routes: Routes = [
  {
    path: "",
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: "dashboard", component: DashboardComponent },
      {
        path: "loads",
        loadComponent: () =>
          import("./features/loads/load-list.component").then((m) => m.LoadListComponent),
      },
    ],
  },
  { path: "login", component: LoginComponent },
];
```

## Authentication

Auth is handled via OIDC:

```typescript
// auth.service.ts
@Injectable({ providedIn: "root" })
export class AuthService {
  private user = signal<User | null>(null);

  isAuthenticated = computed(() => this.user() !== null);

  async login(username: string, password: string) {
    const token = await this.getToken(username, password);
    this.storeToken(token);
    this.user.set(this.decodeToken(token));
  }

  logout() {
    this.clearToken();
    this.user.set(null);
  }
}
```

## Testing

```bash
# Run tests
bun run test

# Watch mode
bun run test:watch
```

## Next Steps

- [Mobile Guide](mobile-guide.md) - Kotlin Multiplatform
- [Backend Guide](backend-guide.md) - .NET patterns
