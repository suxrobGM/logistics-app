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
import { Grid, Stack, Surface, Typography, UiButton, UiDataTable } from "@logistics/shared/ui";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag, PageHeader } from "@/shared/components";

@Component({
  selector: "app-invoice-dashboard",
  templateUrl: "./invoice-dashboard.html",
  imports: [
    CardModule,
    CommonModule,
    CurrencyFormatPipe,
    DateFormatPipe,
    Grid,
    InvoiceStatusTag,
    PageHeader,
    RouterModule,
    SkeletonModule,
    Stack,
    Surface,
    TooltipModule,
    Typography,
    UiButton,
    UiDataTable,
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
