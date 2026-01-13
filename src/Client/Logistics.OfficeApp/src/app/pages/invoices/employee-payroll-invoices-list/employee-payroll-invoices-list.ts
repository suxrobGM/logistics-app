import { CommonModule } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DividerModule } from "primeng/divider";
import { Api, getEmployeeById$Json } from "@/core/api";
import {
  type EmployeeDto,
  type PaymentMethodType,
  type SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";
import { DataContainer, InvoiceStatusTag } from "@/shared/components";
import { EmployeePayrollInvoicesListStore } from "../store/employee-payroll-invoices-list.store";

@Component({
  selector: "app-employee-payroll-invoices-list",
  templateUrl: "./employee-payroll-invoices-list.html",
  providers: [EmployeePayrollInvoicesListStore],
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
export class EmployeePayrollInvoicesListComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeePayrollInvoicesListStore);

  protected readonly employeeId = input.required<string>();
  protected readonly employee = signal<EmployeeDto | null>(null);
  protected readonly isLoadingEmployee = signal(false);

  constructor() {
    // Set the EmployeeId filter when the input changes
    effect(() => {
      const id = this.employeeId();
      if (id) {
        this.store.setFilters({ EmployeeId: id });
        this.fetchEmployee();
      }
    });
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

    const result = await this.api.invoke(getEmployeeById$Json, { userId: this.employeeId() });
    if (result) {
      this.employee.set(result);
    }

    this.isLoadingEmployee.set(false);
  }
}
