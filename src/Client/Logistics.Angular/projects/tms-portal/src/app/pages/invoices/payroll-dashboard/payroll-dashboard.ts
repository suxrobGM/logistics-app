import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoices } from "@logistics/shared/api";
import type { InvoiceDto } from "@logistics/shared/api/models";
import { invoiceStatusOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag } from "@/shared/components";

interface PayrollDashboardData {
  draftCount: number;
  draftAmount: number;
  pendingApprovalCount: number;
  pendingApprovalAmount: number;
  approvedCount: number;
  approvedAmount: number;
  paidCount: number;
  paidAmount: number;
  recentPayrolls: InvoiceDto[];
}

@Component({
  selector: "app-payroll-dashboard",
  templateUrl: "./payroll-dashboard.html",
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
export class PayrollDashboard {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly dashboard = signal<PayrollDashboardData | null>(null);
  protected readonly statusOptions = invoiceStatusOptions;

  constructor() {
    this.fetchDashboard();
  }

  private async fetchDashboard(): Promise<void> {
    this.isLoading.set(true);

    // Fetch payroll invoices grouped by status
    const [draft, pendingApproval, approved, paid, recent] = await Promise.all([
      this.api.invoke(getInvoices, { InvoiceType: "payroll", Status: "draft", PageSize: 1000 }),
      this.api.invoke(getInvoices, { InvoiceType: "payroll", Status: "pending_approval", PageSize: 1000 }),
      this.api.invoke(getInvoices, { InvoiceType: "payroll", Status: "approved", PageSize: 1000 }),
      this.api.invoke(getInvoices, { InvoiceType: "payroll", Status: "paid", PageSize: 1000 }),
      this.api.invoke(getInvoices, { InvoiceType: "payroll", PageSize: 10, OrderBy: "-CreatedAt" }),
    ]);

    const sumAmount = (items: InvoiceDto[] | null | undefined) =>
      items?.reduce((sum, inv) => sum + (inv.total?.amount ?? 0), 0) ?? 0;

    this.dashboard.set({
      draftCount: draft.pagination?.total ?? 0,
      draftAmount: sumAmount(draft.items),
      pendingApprovalCount: pendingApproval.pagination?.total ?? 0,
      pendingApprovalAmount: sumAmount(pendingApproval.items),
      approvedCount: approved.pagination?.total ?? 0,
      approvedAmount: sumAmount(approved.items),
      paidCount: paid.pagination?.total ?? 0,
      paidAmount: sumAmount(paid.items),
      recentPayrolls: recent.items ?? [],
    });

    this.isLoading.set(false);
  }

  getStatusLabel(status: string): string {
    return this.statusOptions.find((opt) => opt.value === status)?.label ?? status;
  }
}
