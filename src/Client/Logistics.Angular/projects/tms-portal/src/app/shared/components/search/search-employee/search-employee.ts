import { Component, ElementRef, inject, input, model, output, signal } from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl, type TenantRoleValue } from "@logistics/shared";
import { Api, getEmployees, type EmployeeDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";

/**
 * Autocomplete for picking an employee by name, optionally narrowed to a set of tenant roles.
 *
 * Implements `FormValueControl` only - see `text-field.ts` for the FormValueControl bridge contract.
 * Never put `formControlName` / `[formField]` on an inner third-party element.
 */
@Component({
  selector: "app-search-employee",
  templateUrl: "./search-employee.html",
  imports: [UiAutocompleteField],
})
export class SearchEmployee implements FormValueControl<EmployeeDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);

  /** The selected employee. Required by `FormValueControl`. */
  public readonly value = model<EmployeeDto | null>(null);

  /** Driven by the forms bridge (Reactive Forms `.disable()`, Signal Forms `disabled()`). */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  /** Restrict results to these exact tenant roles (e.g. `DRIVING_ROLES`). Empty = every role. */
  public readonly roles = input<readonly TenantRoleValue[]>([]);
  public readonly placeholder = input<string>("Type employee name");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected async searchEmployee(event: { query: string }): Promise<void> {
    const roles = this.roles();
    const result = await this.api.invoke(getEmployees, {
      Search: event.query,
      ...(roles.length > 0 ? { Roles: [...roles] } : {}),
    });

    this.suggestedEmployees.set(result?.items ?? []);
  }

  protected clearSelectedEmployee(): void {
    this.value.set(null);
  }
}
