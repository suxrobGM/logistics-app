import { CommonModule } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import {
  Api,
  getEmployeeById,
  type EmployeeDto,
  type PaymentMethodType,
  type SalaryType,
} from "@logistics/shared/api";
import { paymentMethodTypeOptions, salaryTypeOptions } from "@logistics/shared/api/enums";
import {
  Card,
  Divider,
  Grid,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiSortHeader,
  UiTooltip,
} from "@logistics/shared/ui";
import { DataContainer, InvoiceStatusTag, PageHeader } from "@/shared/components";
import { EmployeePayrollInvoicesListStore } from "../store/employee-payroll-invoices-list.store";

@Component({
  selector: "app-employee-payroll-invoices-list",
  templateUrl: "./employee-payroll-invoices-list.html",
  providers: [EmployeePayrollInvoicesListStore],
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DataContainer,
    Divider,
    Grid,
    InvoiceStatusTag,
    PageHeader,
    RouterModule,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    UiSortHeader,
    UiTooltip,
  ],
})
export class EmployeePayrollInvoicesList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeePayrollInvoicesListStore);

  protected readonly employeeId = input.required<string>();
  protected readonly employee = signal<EmployeeDto | null>(null);
  protected readonly isLoadingEmployee = signal(false);

  private lastLoadedEmployeeId: string | null = null;

  constructor() {
    // Set the EmployeeId filter when the input changes
    effect(
      () => {
        const id = this.employeeId();
        // Only load if the employeeId has changed to prevent infinite loops
        if (id && id !== this.lastLoadedEmployeeId) {
          this.lastLoadedEmployeeId = id;
          this.store.setFilters({ EmployeeId: id });
          this.fetchEmployee();
        }
      },
      { allowSignalWrites: true },
    );
  }

  protected addInvoice(): void {
    this.router.navigate(["/invoices/payroll/add"]);
  }

  protected getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }

    return (
      paymentMethodTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown"
    );
  }

  protected getSalaryTypeDesc(enumValue?: SalaryType): string {
    if (!enumValue) return "N/A";
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown";
  }

  private async fetchEmployee(): Promise<void> {
    this.isLoadingEmployee.set(true);

    const result = await this.api.invoke(getEmployeeById, { userId: this.employeeId() });
    if (result) {
      this.employee.set(result);
    }

    this.isLoadingEmployee.set(false);
  }
}
