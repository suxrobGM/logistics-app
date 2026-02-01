/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, input, model, output, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { TenantRole, type TenantRoleValue } from "@logistics/shared";
import { isEmptyGuid } from "@logistics/shared";
import { Api, type EmployeeDto, getEmployeeById, getEmployees } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

/**
 * Component for searching and selecting an employee.
 * This component uses an autocomplete input to allow users to search for employees by name.
 * It accepts an employee ID or an EmployeeDto object as input and emits the selected employee.
 * Supports filtering by role (e.g., "Driver", "Dispatcher").
 */
@Component({
  selector: "app-search-employee",
  templateUrl: "./search-employee.html",
  imports: [AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchEmployee),
      multi: true,
    },
  ],
})
export class SearchEmployee implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly disabled = signal<boolean>(false);

  /** Filter employees by role (e.g., "Driver", "Dispatcher") */
  public readonly role = input<TenantRoleValue | null>(null);
  public readonly placeholder = input<string>("Type employee name");

  public readonly selectedEmployee = model<EmployeeDto | null>(null);
  public readonly selectedEmployeeChange = output<EmployeeDto | null>();

  protected async searchEmployee(event: { query: string }): Promise<void> {
    const roleValue = this.role() as string;
    const result = await this.api.invoke(getEmployees, {
      Search: event.query,
      ...(roleValue ? { Role: roleValue } : {}),
    });

    this.suggestedEmployees.set(result?.items ?? []);
  }

  protected changeSelectedEmployee(event: AutoCompleteSelectEvent): void {
    this.selectedEmployeeChange.emit(event.value);
    this.onChange(event.value);
  }

  private async fetchEmployeeById(id: string): Promise<void> {
    if (isEmptyGuid(id)) {
      this.selectedEmployee.set(null);
      return;
    }

    const result = await this.api.invoke(getEmployeeById, { userId: id });
    if (result) {
      this.selectedEmployee.set(result);
      this.onChange(result);
    }
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: EmployeeDto | null): void {}
  private onTouched(): void {}

  writeValue(value: EmployeeDto | string | null): void {
    if (typeof value === "string") {
      this.fetchEmployeeById(value);
      return;
    }

    this.selectedEmployee.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  //#endregion
}
