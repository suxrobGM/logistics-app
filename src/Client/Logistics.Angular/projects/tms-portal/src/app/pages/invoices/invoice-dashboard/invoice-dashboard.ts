import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import {
  Api,
  getInvoiceDashboard,
  type InvoiceDashboardDto,
  type InvoiceDto,
} from "@logistics/shared/api";
import { Grid, Stack, Surface, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-invoice-dashboard",
  templateUrl: "./invoice-dashboard.html",
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    TableModule,
    SkeletonModule,
    TooltipModule,
    CurrencyFormatPipe,
    DateFormatPipe,
    InvoiceStatusTag,
    Grid,
    Stack,
    Surface,
    Typography,
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
