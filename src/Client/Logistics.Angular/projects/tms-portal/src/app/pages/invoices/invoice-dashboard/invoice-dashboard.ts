import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import {
  Api,
  getInvoiceDashboard,
  type InvoiceDashboardDto,
  type InvoiceDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  Grid,
  Skeleton,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDataTable,
  UiTooltip,
} from "@logistics/shared/ui";
import { InvoiceStatusTag, PageHeader } from "@/shared/components";

@Component({
  selector: "app-invoice-dashboard",
  templateUrl: "./invoice-dashboard.html",
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DateFormatPipe,
    Grid,
    InvoiceStatusTag,
    PageHeader,
    RouterModule,
    Skeleton,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class InvoiceDashboard {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly dashboard = signal<InvoiceDashboardDto | null>(null);

  constructor() {
    this.fetchDashboard();
  }

  private async fetchDashboard(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getInvoiceDashboard, {});
    if (result) {
      this.dashboard.set(result);
    }

    this.isLoading.set(false);
  }

  getInvoiceLink(invoice: InvoiceDto): string {
    return `/invoices/loads/${invoice.loadId}/${invoice.id}`;
  }
}
