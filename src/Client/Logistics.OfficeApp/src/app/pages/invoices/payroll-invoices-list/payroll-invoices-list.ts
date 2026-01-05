import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getInvoices$Json } from "@/core/api";
import  { type InvoiceDto, type SalaryType, salaryTypeOptions } from "@/core/api/models";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-payroll-invoices-list",
  templateUrl: "./payroll-invoices-list.html",
  styleUrls: [],
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    InputTextModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    IconFieldModule,
    InputIconModule,
  ],
})
export class PayrollInvoicesListComponent {
  private readonly api = inject(Api);

  protected readonly invoices = signal<InvoiceDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  async search(event: Event): Promise<void> {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getInvoices$Json, { EmployeeName: searchValue });
    if (result.success && result.data) {
      this.invoices.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getInvoices$Json, {
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      InvoiceType: "payroll",
    });
    if (result.success && result.data) {
      this.invoices.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
