import { Component, inject, input, output, signal, type OnInit } from "@angular/core";
import { Api, getEmployees, type EmployeeDto } from "@logistics/shared/api";
import { UiAutocompleteField, UiMultiSelectField } from "@logistics/shared/ui";

@Component({
  selector: "app-payroll-employee-selector",
  templateUrl: "./employee-selector.html",
  imports: [UiAutocompleteField, UiMultiSelectField],
})
export class PayrollEmployeeSelector implements OnInit {
  private readonly api = inject(Api);

  public readonly mode = input<"single" | "multi">("single");
  public readonly placeholder = input("Select employee");
  public readonly employeeSelect = output<EmployeeDto>();
  public readonly employeesChange = output<EmployeeDto[]>();

  protected readonly suggestedEmployees = signal<EmployeeDto[]>([]);
  protected readonly allEmployees = signal<EmployeeDto[]>([]);
  protected readonly selectedEmployeesValue = signal<EmployeeDto[]>([]);
  /** Signal-backed so it can two-way bind to `ui-autocomplete-field`'s `value` model. */
  protected readonly selectedEmployeeValue = signal<EmployeeDto | null>(null);

  async ngOnInit(): Promise<void> {
    if (this.mode() === "multi") {
      await this.loadAllEmployees();
    }
  }

  async searchEmployee(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getEmployees, { Search: event.query });
    if (result.items) {
      this.suggestedEmployees.set(result.items);
    }
  }

  onEmployeeSelect(employee: EmployeeDto | null): void {
    if (employee) {
      this.employeeSelect.emit(employee);
    }
  }

  onMultiSelectChange(employees: EmployeeDto[]): void {
    this.selectedEmployeesValue.set(employees);
    this.employeesChange.emit(employees);
  }

  setEmployee(employee: EmployeeDto | null): void {
    this.selectedEmployeeValue.set(employee);
  }

  setEmployees(employees: EmployeeDto[]): void {
    this.selectedEmployeesValue.set(employees);
  }

  private async loadAllEmployees(): Promise<void> {
    const result = await this.api.invoke(getEmployees, { PageSize: 500 });
    if (result.items) {
      this.allEmployees.set(result.items);
    }
  }
}
