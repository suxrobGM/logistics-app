import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import type { CustomerDto, InvoiceDto, InvoiceStatus } from "@logistics/shared/api";
import { invoiceStatusOptions } from "@logistics/shared/api/enums";
import type { SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { MultiSelectModule } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { ToolbarModule } from "primeng/toolbar";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import {
  DataContainer,
  DateRangePicker,
  InvoiceStatusTag,
  LabeledField,
  SearchCustomer,
  SearchInput,
} from "@/shared/components";
import { SendInvoiceDialog } from "../components";
import { LoadInvoicesListStore } from "../store/load-invoices-list.store";

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.html",
  providers: [LoadInvoicesListStore],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    DataContainer,
    CheckboxModule,
    ToolbarModule,
    SendInvoiceDialog,
    MultiSelectModule,
    DateRangePicker,
    SearchInput,
    SearchCustomer,
    LabeledField,
  ],
})
export class LoadInvoicesList implements OnInit {
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(LoadInvoicesListStore);
  protected readonly loadId = input<string>();

  // Filter state
  protected readonly selectedStatuses = signal<InvoiceStatus[]>([]);
  protected readonly selectedCustomer = signal<CustomerDto | null>(null);
  protected readonly overdueOnly = signal<boolean>(false);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Filter options
  protected readonly statusOptions: SelectItem[] = invoiceStatusOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedCustomer()) count++;
    if (this.overdueOnly()) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  // UI state
  protected readonly selectedInvoices = signal<InvoiceDto[]>([]);
  protected readonly showSendInvoiceDialog = signal(false);
  protected readonly selectedInvoiceId = signal<string | null>(null);

  // Check if we're viewing invoices for a specific load
  protected readonly isLoadSpecific = computed(() => !!this.loadId());

  ngOnInit(): void {
    const id = this.loadId();
    if (id) {
      this.store.setFilters({ LoadId: id });
    } else {
      this.store.load();
    }
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    // Preserve LoadId if set
    const loadId = this.loadId();
    if (loadId) {
      filters["LoadId"] = loadId;
    }

    // Status filter (take first if multiple - API typically expects single value)
    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Status"] = statuses[0];
    }

    // Customer filter
    const customer = this.selectedCustomer();
    if (customer?.id) {
      filters["CustomerId"] = customer.id;
    }

    // Overdue only filter
    if (this.overdueOnly()) {
      filters["OverdueOnly"] = true;
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
    this.selectedCustomer.set(null);
    this.overdueOnly.set(false);
    this.dateRange.set(null);

    // Preserve LoadId if set
    const loadId = this.loadId();
    this.store.setFilters(loadId ? { LoadId: loadId } : {});
  }

  protected onDateRangeChange(dates: Date[]): void {
    this.dateRange.set(dates);
    this.applyFilters();
  }

  onSelectionChange(invoices: InvoiceDto[]): void {
    this.selectedInvoices.set(invoices);
  }

  clearSelection(): void {
    this.selectedInvoices.set([]);
  }

  openSendDialog(invoice: InvoiceDto): void {
    this.selectedInvoiceId.set(invoice.id ?? null);
    this.showSendInvoiceDialog.set(true);
  }

  onInvoiceSent(): void {
    this.toastService.showSuccess("Invoice sent successfully");
    this.store.load();
  }

  isOverdue(invoice: InvoiceDto): boolean {
    if (!invoice.dueDate || invoice.status === "paid") return false;
    return new Date(invoice.dueDate) < new Date();
  }
}
