import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getInvoices$Json, getEmployeeById$Json } from "@/core/api";
import {
  type EmployeeDto,
  type InvoiceDto,
  type PaymentMethodType,
  type SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-employee-payroll-invoices-list",
  templateUrl: "./employee-payroll-invoices-list.html",
  imports: [
    CommonModule,
    CardModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    RouterModule,
    InvoiceStatusTag,
    ProgressSpinnerModule,
  ],
})
export class EmployeePayrollInvoicesListComponent implements OnInit {
  private readonly api = inject(Api);

  protected readonly employeeId = input.required<string>();
  protected readonly invoices = signal<InvoiceDto[]>([]);
  protected readonly employee = signal<EmployeeDto | null>(null);
  protected readonly isLoadingEmployee = signal(false);
  protected readonly isLoadingPayrolls = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  ngOnInit(): void {
    this.fetchEmployee();
  }

  protected async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoadingPayrolls.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getInvoices$Json, {
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      EmployeeId: this.employeeId(),
    });
    if (result.success && result.data) {
      this.invoices.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoadingPayrolls.set(false);
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
    if (result.data) {
      this.employee.set(result.data);
    }

    this.isLoadingEmployee.set(false);
  }
}
