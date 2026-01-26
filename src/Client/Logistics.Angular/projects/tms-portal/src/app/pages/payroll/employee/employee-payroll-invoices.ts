import { CommonModule } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, getEmployeeById } from "@logistics/shared/api";
import { type EmployeeDto, type PaymentMethodType, type SalaryType } from "@logistics/shared/api";
import { paymentMethodTypeOptions, salaryTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, InvoiceStatusTag } from "@/shared/components";
import { EmployeePayrollListStore } from "../store/employee-list.store";

@Component({
  selector: "app-employee-payroll-invoices",
  templateUrl: "./employee-payroll-invoices.html",
  providers: [EmployeePayrollListStore],
  imports: [
    CommonModule,
    CardModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    RouterModule,
    InvoiceStatusTag,
    ProgressSpinnerModule,
    DataContainer,
    DividerModule,
  ],
})
export class EmployeePayrollInvoices {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeePayrollListStore);

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
    this.router.navigate(["/payroll/invoices/add"]);
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
