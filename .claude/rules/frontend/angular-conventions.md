---
paths:
  - "src/Client/Logistics.Angular/**/*.ts"
  - "src/Client/Logistics.Angular/**/*.html"
---

# Angular Code Conventions

## Component Structure

- Use standalone components (always `standalone: true`)
- Separate HTML template files (`templateUrl`, not inline `template`)
- Component files named `{name}.ts` (not `{name}.component.ts`)
- Template files named `{name}.html`

## Component Prefixes

| Project | Prefix |
|---------|--------|
| tms-portal | `app-` |
| customer-portal | `cp-` |
| website | `web-` |
| shared | `ui-` |

## Signals & Reactivity

- Use `signal()` for local component state
- Use `computed()` for derived state
- Use `input()` and `output()` functions (NOT `@Input`/`@Output` decorators)
- Use `@ngrx/signals` stores for complex state management

```typescript
// Correct
public readonly data = input<DataDto>();
public readonly dataChange = output<DataDto>();
protected readonly isLoading = signal(false);
protected readonly items = computed(() => this.store.items());

// Wrong - do not use decorators
@Input() data: DataDto;
@Output() dataChange = new EventEmitter<DataDto>();
```

## Dependency Injection

- Use `inject()` function (not constructor injection)
- Mark injected services as `private readonly`

```typescript
private readonly api = inject(Api);
private readonly toastService = inject(ToastService);
protected readonly store = inject(MyStore);
```

## Template Syntax

- Use native control flow: `@if`, `@for`, `@switch` (NOT `*ngIf`, `*ngFor`)
- Use `@empty` block for empty states in `@for`
- Avoid complex nested ternary expressions in templates

```html
@if (isLoading()) {
  <p-progressSpinner />
} @else {
  @for (item of items(); track item.id) {
    <app-item [data]="item" />
  } @empty {
    <p>No items found</p>
  }
}
```

## Access Modifiers

- `protected` for properties/methods used in templates
- `private` for internal logic
- `readonly` for injected dependencies and signals

## Imports

- Import shared library: `import { X } from "@logistics/shared";`
- Import API models: `import type { XDto } from "@logistics/shared/api/models";`
- Import within app: `import { X } from "@/core/services";`

## Styling

- Use Tailwind CSS utility classes for styling
- Avoid custom CSS unless necessary
- Use `class` attribute for static classes and avoid `[ngClass]` when possible

## Inline Templates & Styles

- Avoid inline templates and styles in component decorators
- Always use external files for better readability and maintainability
- Don't use `standalone: true` attribute to the component decorator because it's redundant and on Angular 20+ it's default behavior.
