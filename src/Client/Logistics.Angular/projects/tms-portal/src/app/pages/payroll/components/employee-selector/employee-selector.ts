import { Component, type OnInit, inject, input, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Api, getEmployees } from "@logistics/shared/api";
import type { EmployeeDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { MultiSelectModule } from "primeng/multiselect";

@Component({
  selector: "app-payroll-employee-selector",
  templateUrl: "./employee-selector.html",
  imports: [FormsModule, AutoCompleteModule, MultiSelectModule],
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
  protected selectedEmployeeValue: EmployeeDto | null = null;

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

  onEmployeeSelect(event: AutoCompleteSelectEvent): void {
    const employee = event.value as EmployeeDto;
    this.employeeSelect.emit(employee);
  }

  onMultiSelectChange(employees: EmployeeDto[]): void {
    this.selectedEmployeesValue.set(employees);
    this.employeesChange.emit(employees);
  }

  setEmployee(employee: EmployeeDto | null): void {
    this.selectedEmployeeValue = employee;
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
