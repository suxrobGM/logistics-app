import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import type { InvoiceDto, InvoiceStatus } from "@logistics/shared/api";
import { invoiceStatusOptions } from "@logistics/shared/api/enums";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  Grid,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiMultiSelectField,
  UiSortHeader,
  UiTooltip,
} from "@logistics/shared/ui";
import {
  DataContainer,
  DateRangePicker,
  InvoiceStatusTag,
  PageHeader,
  SearchField,
  UiFormField,
} from "@/shared/components";
import { LoadInvoicesListStore } from "../store/load-invoices-list.store";

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.html",
  providers: [LoadInvoicesListStore],
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DataContainer,
    DateFormatPipe,
    DateRangePicker,
    Grid,
    InvoiceStatusTag,
    PageHeader,
    RouterModule,
    SearchField,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiFormField,
    UiMultiSelectField,
    UiSortHeader,
    UiTooltip,
  ],
})
export class LoadInvoicesList {
  protected readonly store = inject(LoadInvoicesListStore);

  // Filter state
  protected readonly selectedStatuses = signal<InvoiceStatus[]>([]);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Filter options
  protected readonly statusOptions = invoiceStatusOptions;

  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Status"] = statuses.join(",");
    }

    const range = this.dateRange();
    if (range?.length === 2) {
      filters["StartDate"] = range[0].toISOString();
      filters["EndDate"] = range[1].toISOString();
    }

    this.store.setFilters(filters);
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.dateRange.set(null);
    this.store.setFilters({});
  }

  protected onDateRangeChange(dates: Date[]): void {
    this.dateRange.set(dates);
    this.applyFilters();
  }

  protected getInvoiceLink(invoice: InvoiceDto): string {
    return `/invoices/loads/${invoice.loadId}/${invoice.id}`;
  }
}
