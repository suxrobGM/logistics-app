import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, type PortalInvoiceDto, getPortalInvoices } from "@logistics/shared/api";
import { MessageService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { ToastModule } from "primeng/toast";

@Component({
  selector: "cp-invoices-list",
  templateUrl: "./invoices-list.html",
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    TableModule,
    ButtonModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    ProgressSpinnerModule,
    ToastModule,
  ],
})
export class InvoicesList {
  private readonly api = inject(Api);
  private readonly messageService = inject(MessageService);

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

  protected onLazyLoad(event: TableLazyLoadEvent): void {
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

  protected getStatusSeverity(status: string | undefined): "success" | "info" | "warn" | "danger" {
    switch (status) {
      case "paid":
        return "success";
      case "partially_paid":
        return "info";
      case "issued":
        return "warn";
      case "cancelled":
        return "danger";
      default:
        return "info";
    }
  }

  protected downloadInvoice(invoice: PortalInvoiceDto): void {
    // TODO: Implement actual PDF download when backend endpoint is available
    this.messageService.add({
      severity: "info",
      summary: "Download",
      detail: `Invoice INV-${invoice.number} download will be available soon.`,
    });
  }
}
