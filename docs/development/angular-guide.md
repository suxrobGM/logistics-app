# Angular Development Guide

Patterns and conventions for the Office App (Angular 21).

## Project Structure

```text
src/Client/Logistics.OfficeApp/
├── src/
│   ├── app/
│   │   ├── core/              # Singleton services
│   │   │   ├── auth/          # Authentication
│   │   │   ├── api/           # Generated API client
│   │   │   └── services/      # Core services
│   │   ├── shared/            # Shared components
│   │   │   ├── components/    # Reusable components
│   │   │   └── pipes/         # Custom pipes
│   │   ├── pages/          # Feature modules
│   │   │   ├── dashboard/
│   │   │   ├── loads/
│   │   │   ├── customers/
│   │   │   └── ...
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   └── environments/
└── package.json
```

## Key Patterns

### Standalone Components Only

No NgModules. All components are standalone:

```typescript
@Component({
  selector: 'app-load-list',
  standalone: true,
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

This reads the OpenAPI spec from the running API and generates typed services.

### Using API Services

```typescript
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

## Common Commands

```bash
# Development server
bun run start

# Production build
bun run build

# Linting
bun run lint

# Format code
bun run format

# Generate API client
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
