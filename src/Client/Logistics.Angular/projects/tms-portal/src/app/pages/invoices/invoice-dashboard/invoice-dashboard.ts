import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoiceDashboard } from "@logistics/shared/api";
import type { InvoiceDashboardDto, InvoiceDto } from "@logistics/shared/api";
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
    CurrencyPipe,
    DatePipe,
    InvoiceStatusTag,
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
