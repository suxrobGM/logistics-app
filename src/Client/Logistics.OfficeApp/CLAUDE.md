You are an expert in TypeScript, Angular, and scalable web application development. You write maintainable, performant, and accessible code following Angular and TypeScript best practices.

## TypeScript Best Practices

- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

## Angular Best Practices

- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images.
  - `NgOptimizedImage` does not work for inline base64 images.

## Components

- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead

## Route Parameters

Use signal inputs for route parameters instead of `ActivatedRoute`. Angular's router with `withComponentInputBinding()` automatically binds route params to matching input names.

```typescript
// DO: Use signal inputs for route parameters
readonly employeeId = input.required<string>();

ngOnInit(): void {
  const id = this.employeeId();
  this.loadData(id);
}

// DON'T: Use ActivatedRoute snapshot
private readonly route = inject(ActivatedRoute);

ngOnInit(): void {
  const id = this.route.snapshot.paramMap.get("employeeId");  // Don't do this
  this.loadData(id);
}
```

Benefits:

- Cleaner, more declarative code
- Works with signals ecosystem
- Automatically typed (no null checks needed with `input.required`)
- No need to inject `ActivatedRoute`

## State Management

- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead

## Templates

- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Use self-closing tags for components without children: `<p-button />` not `<p-button></p-button>`

## Services

- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Use the `inject()` function instead of constructor injection

## List Pages Pattern

List pages must use the `createListStore` factory and `DataContainer` component for consistent error handling, loading states, and state management.

### Creating a List Store

```typescript
// src/app/pages/customers/store/customers-list.store.ts
import { createListStore } from "@logistics/shared/stores";
import { getCustomers } from "@/core/api";
import type { CustomerDto } from "@/core/api/models";

export const CustomersListStore = createListStore<CustomerDto>(getCustomers, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
```

### Using the Store in a Component

```typescript
@Component({
  selector: "app-customers-list",
  templateUrl: "./customers-list.html",
  providers: [CustomersListStore],  // Provide the store
  imports: [DataContainer, TableModule, ...],
})
export class CustomersListComponent {
  protected readonly store = inject(CustomersListStore);

  protected search(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.store.setSearch(value);
  }

  protected addCustomer(): void {
    this.router.navigate(["/customers/add"]);
  }
}
```

### Template Pattern

```html
<p-card>
  <app-data-container
    [loading]="store.isLoading()"
    [error]="store.error()"
    [isEmpty]="store.isEmpty()"
    skeletonVariant="table"
    [skeletonRows]="10"
    emptyTitle="No customers yet"
    emptyMessage="Add your first customer to get started."
    emptyActionLabel="Add Customer"
    (onRetry)="store.retry()"
    (onEmptyAction)="addCustomer()"
  >
    <p-table
      [value]="store.data()"
      [lazy]="true"
      [paginator]="true"
      (onLazyLoad)="store.onLazyLoad($event)"
      [rows]="store.pageSize()"
      [first]="store.first()"
      [totalRecords]="store.totalRecords()"
      [loading]="store.isLoading()"
    >
      <!-- table content -->
    </p-table>
  </app-data-container>
</p-card>
```

### Store Methods Available

- `load()` - Load data from API
- `onLazyLoad(event)` - Handle PrimeNG table lazy load events
- `setSearch(value)` - Set search query and reload
- `setSort(field, order)` - Set sort and reload
- `setFilters(filters)` - Set additional filters and reload
- `retry()` - Retry the last failed operation
- `reset()` - Reset store to initial state
- `removeItem(id)` - Optimistically remove item from list
- `updateItem(id, updates)` - Optimistically update item in list

## Form Fields

Use the `LabeledField` component for form inputs instead of manually adding labels, hints, and error handling.

```html
<!-- DO: Use LabeledField component -->
<app-labeled-field
  label="Email"
  for="email"
  [required]="true"
  hint="We'll never share your email"
  [control]="form.controls.email"
>
  <input pInputText id="email" formControlName="email" class="w-full" />
</app-labeled-field>

<!-- DON'T: Manually structure form fields -->
<div class="mb-4">
  <label for="email" class="mb-2 block text-sm font-medium">Email</label>
  <input pInputText id="email" formControlName="email" class="w-full" />
  <small class="text-gray-500">We'll never share your email</small>
</div>
```

LabeledField inputs:

- `label` - Label text
- `for` - The field id for the label
- `required` - Shows required indicator (\*)
- `hint` - Help text shown below the field
- `control` - The form control for validation display

## Error Handling

- HTTP errors are automatically categorized by `ErrorHandlerService` (network, auth, validation, server)
- Errors are displayed via toast notifications automatically
- Use `AppError` type from `@/core/errors` for typed error handling
- Retryable errors (network, server) show retry buttons in `ErrorState` component

## Toast and Confirmation Dialogs

Use `ToastService` from `@/core/services` for toast notifications and confirmation dialogs. Do NOT use PrimeNG's `ConfirmationService` or `MessageService` directly.

```typescript
// DO: Use ToastService
import { ToastService } from "@/core/services";

private readonly toastService = inject(ToastService);

// Show confirmation dialog
this.toastService.confirm({
  message: "Are you sure you want to delete this item?",
  header: "Confirm Delete",
  icon: "pi pi-exclamation-triangle",
  acceptButtonStyleClass: "p-button-danger",
  accept: () => this.deleteItem(),
});

// Show toast messages
this.toastService.success("Item saved successfully");
this.toastService.error("Failed to save item");
this.toastService.warn("This action cannot be undone");

// DON'T: Use ConfirmationService directly
import { ConfirmationService } from "primeng/api";  // Don't do this
```

The `ToastService` is a singleton that wraps `ConfirmationService` and `MessageService`, using a global confirm dialog. Components do not need to include `ConfirmDialogModule` or `<p-confirmDialog />` in their templates.

## DTO and HTTP Layer

- Use generated API clients from ng-openapi-gen
- Do NOT write custom HTTP clients unless absolutely necessary
- Use DTOs directly in components and services; avoid mapping to domain models unless needed for complex logic
- Run `bun run gen:api` to regenerate API clients after OpenAPI spec changes
