import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  getPortalInvoices,
  getPortalLoads,
  getPortalProfile,
  type CustomerUserDto,
  type PortalLoadDto,
} from "@logistics/shared/api";
import { Grid, Icon, Stack, StatusBadge, Surface, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TenantContextService } from "@/core/services";

@Component({
  selector: "cp-dashboard",
  templateUrl: "./dashboard.html",
  imports: [
    RouterLink,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    SkeletonModule,
    TableModule,
    Grid,
    Icon,
    Stack,
    StatusBadge,
    Surface,
    Typography,
  ],
})
export class Dashboard {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantContextService);

  protected readonly isLoading = signal(true);
  protected readonly profile = signal<CustomerUserDto | null>(null);
  protected readonly recentLoads = signal<PortalLoadDto[]>([]);
  protected readonly activeLoadCount = signal(0);
  protected readonly pendingInvoiceCount = signal(0);
  protected readonly companyName = signal<string | null>(null);

  constructor() {
    this.loadDashboardData();
  }

  private async loadDashboardData(): Promise<void> {
    try {
      const tenant = this.tenantService.currentTenant();
      this.companyName.set(tenant?.companyName ?? tenant?.tenantName ?? null);

      const [profile, loadsResult, invoicesResult] = await Promise.all([
        this.api.invoke(getPortalProfile),
        this.api.invoke(getPortalLoads, { OnlyActiveLoads: true, PageSize: 5 }),
        this.api.invoke(getPortalInvoices, { PageSize: 100 }),
      ]);

      this.profile.set(profile);
      this.recentLoads.set(loadsResult.items ?? []);
      this.activeLoadCount.set(loadsResult.pagination?.total ?? 0);

      // Count unpaid invoices (issued but not paid)
      const unpaidInvoices = (invoicesResult.items ?? []).filter(
        (inv) => inv.status === "issued" || inv.status === "partially_paid",
      );
      this.pendingInvoiceCount.set(unpaidInvoices.length);
    } catch (error) {
      console.error("Failed to load dashboard data:", error);
    } finally {
      this.isLoading.set(false);
    }
  }
}
