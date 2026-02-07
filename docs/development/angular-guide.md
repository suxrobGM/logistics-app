# Angular Development Guide

Patterns and conventions for the Angular workspace (Angular 21).

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
│   │   │   └── interceptors/       # HTTP interceptors
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

| Project | Prefix | Port | Description |
|---------|--------|------|-------------|
| @logistics/shared | `ui-` | N/A | Shared library (API, services, pipes) |
| @logistics/admin-portal | `adm-` | 7002 | Super admin management |
| @logistics/tms-portal | `app-` | 7003 | Internal TMS for dispatchers |
| @logistics/customer-portal | `cp-` | 7004 | Customer self-service portal |
| @logistics/website | `web-` | 7005 | Marketing website (SSR) |

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
  selector: 'app-load-list',
  imports: [CommonModule, TableModule, ButtonModule],
  templateUrl: './load-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadListComponent { }
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
  <p-progressSpinner />
} @else if (error()) {
  <p-message severity="error" [text]="error()" />
} @else {
  @for (load of loads(); track load.id) {
    <app-load-card [load]="load" />
  } @empty {
    <p>No loads found</p>
  }
}

<!-- Avoid -->
<p-progressSpinner *ngIf="loading"></p-progressSpinner>
<div *ngFor="let load of loads">...</div>
```

### Inject Function

Use `inject()` instead of constructor injection:

```typescript
@Component({ ... })
export class LoadListComponent {
  private loadService = inject(LoadService);
  private router = inject(Router);
  private messageService = inject(MessageService);

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

### Reactive Forms

Prefer reactive forms:

```typescript
@Component({ ... })
export class LoadFormComponent {
  private fb = inject(FormBuilder);

  form = this.fb.group({
    customerId: ['', Validators.required],
    origin: ['', [Validators.required, Validators.maxLength(200)]],
    destination: ['', [Validators.required, Validators.maxLength(200)]],
    weight: [0, [Validators.min(0)]]
  });

  onSubmit() {
    if (this.form.valid) {
      // Submit form.getRawValue()
    }
  }
}
```

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
import { LoadsApiService, CustomersApiService } from '@logistics/shared/api';

// Error handling
import { ErrorHandlerService } from '@logistics/shared/errors';

// Common services
import { ToastService, HttpCacheService } from '@logistics/shared/services';
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

## PrimeNG Components

The app uses PrimeNG for UI components:

```typescript
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
```

## Routing

Routes are defined in `app.routes.ts`:

```typescript
export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      {
        path: 'loads',
        loadComponent: () => import('./features/loads/load-list.component')
          .then(m => m.LoadListComponent)
      }
    ]
  },
  { path: 'login', component: LoginComponent }
];
```

## Authentication

Auth is handled via OIDC:

```typescript
// auth.service.ts
@Injectable({ providedIn: 'root' })
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
