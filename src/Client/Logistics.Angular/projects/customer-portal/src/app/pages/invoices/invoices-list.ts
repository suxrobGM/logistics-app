import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, getPortalInvoices, type PortalInvoiceDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import type { ListLazyLoadEvent } from "@logistics/shared/stores";
import {
  Icon,
  SearchField,
  Stack,
  StatusBadge,
  Surface,
  Typography,
  UiButton,
  UiDataTable,
} from "@logistics/shared/ui";

@Component({
  selector: "cp-invoices-list",
  templateUrl: "./invoices-list.html",
  imports: [
    CurrencyFormatPipe,
    DateFormatPipe,
    Icon,
    RouterLink,
    SearchField,
    Stack,
    StatusBadge,
    Surface,
    Typography,
    UiButton,
    UiDataTable,
  ],
})
export class InvoicesList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly invoices = signal<PortalInvoiceDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly isLoading = signal(true);
  protected readonly searchQuery = signal("");
  protected readonly tableFirst = signal(0);

  private currentPage = 1;
  private pageSize = 10;

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getPortalInvoices, {
        Page: this.currentPage,
        PageSize: this.pageSize,
        Search: this.searchQuery() || undefined,
      });

      this.invoices.set(result.items ?? []);
      this.totalRecords.set(result.pagination?.total ?? 0);
    } catch (error) {
      console.error("Failed to load invoices:", error);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onLazyLoad(event: ListLazyLoadEvent): void {
    this.currentPage = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.pageSize = event.rows ?? 10;
    this.loadData();
  }

  protected onSearch(query: string): void {
    this.searchQuery.set(query);
    this.currentPage = 1;
    this.tableFirst.set(0);
    this.loadData();
  }

  protected downloadInvoice(invoice: PortalInvoiceDto): void {
    // TODO: Implement actual PDF download when backend endpoint is available
    this.toastService.showInfo(
      `Invoice INV-${invoice.number} download will be available soon.`,
      "Download",
    );
  }
}
