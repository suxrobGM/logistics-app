import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import { invoiceStatusOptions } from "@logistics/shared/api/enums";
import {
  type InvoiceStatus,
  type SalaryType,
  salaryTypeOptions,
} from "@logistics/shared/api/models";
import type { SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { MultiSelectModule } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, InvoiceStatusTag, LabeledField, SearchInput } from "@/shared/components";
import { type DatePreset, getDatePreset } from "@/shared/utils";
import { PayrollInvoicesListStore } from "../store/payroll-invoices-list.store";

@Component({
  selector: "app-payroll-invoices-list",
  templateUrl: "./payroll-invoices-list.html",
  providers: [PayrollInvoicesListStore],
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    CardModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    DataContainer,
    MultiSelectModule,
    DatePickerModule,
    SearchInput,
    LabeledField,
  ],
})
export class PayrollInvoicesList {
  private readonly router = inject(Router);
  protected readonly store = inject(PayrollInvoicesListStore);

  // Filter state
  protected readonly selectedStatuses = signal<InvoiceStatus[]>([]);
  protected readonly selectedSalaryTypes = signal<SalaryType[]>([]);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Filter options
  protected readonly statusOptions: SelectItem[] = invoiceStatusOptions;
  protected readonly salaryTypeOptions: SelectItem[] = salaryTypeOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedSalaryTypes().length > 0) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    // Status filter
    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Status"] = statuses[0];
    }

    // Salary type filter
    const salaryTypes = this.selectedSalaryTypes();
    if (salaryTypes.length > 0) {
      filters["SalaryType"] = salaryTypes[0];
    }

    // Date range filter
    const range = this.dateRange();
    if (range?.length === 2) {
      filters["StartDate"] = range[0].toISOString();
      filters["EndDate"] = range[1].toISOString();
    }

    this.store.setFilters(filters);
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedSalaryTypes.set([]);
    this.dateRange.set(null);
    this.store.setFilters({});
  }

  protected setDatePreset(preset: DatePreset): void {
    this.dateRange.set(getDatePreset(preset));
    this.applyFilters();
  }

  protected addInvoice(): void {
    this.router.navigate(["/invoices/payroll/add"]);
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
