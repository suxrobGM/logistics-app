import { Component, ElementRef, inject, input, model, output, signal } from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl, type TenantRoleValue } from "@logistics/shared";
import { Api, getEmployees, type EmployeeDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";

/**
 * Component for searching and selecting an employee.
 * This component uses an autocomplete input to allow users to search for employees by name.
 * It emits the selected employee via its `value` model. Supports filtering by role
 * (e.g., "Driver", "Dispatcher").
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
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

  /** Filter employees by role (e.g., "Driver", "Dispatcher") */
  public readonly role = input<TenantRoleValue | null>(null);
  public readonly placeholder = input<string>("Type employee name");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected async searchEmployee(event: { query: string }): Promise<void> {
    const roleValue = this.role() as string;
    const result = await this.api.invoke(getEmployees, {
      Search: event.query,
      ...(roleValue ? { Role: roleValue } : {}),
    });

    this.suggestedEmployees.set(result?.items ?? []);
  }

  protected clearSelectedEmployee(): void {
    this.value.set(null);
  }
}
