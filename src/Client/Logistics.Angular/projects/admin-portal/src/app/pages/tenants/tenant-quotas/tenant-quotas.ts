import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import {
  Api,
  formatSortField,
  getTenantQuotaUsages,
  resetTenantQuotas,
  type TenantQuotaUsageDto,
} from "@logistics/shared/api";
import type { PagedResponse } from "@logistics/shared/api/models";
import { BaseTable, PageHeader, type TableQueryParams } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressBar } from "primeng/progressbar";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";

@Component({
  selector: "adm-tenant-quotas",
  templateUrl: "./tenant-quotas.html",
  imports: [
    ButtonModule,
    CardModule,
    ProgressBar,
    TableModule,
    TagModule,
    TooltipModule,
    DatePipe,
    PageHeader,
    CurrencyPipe,
  ],
})
export class TenantQuotas extends BaseTable<TenantQuotaUsageDto> {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isResetting = signal(false);
  protected readonly selectedTenantIds = signal<Set<string>>(new Set());

  protected readonly allSelected = computed(() => {
    const items = this.data();
    return items.length > 0 && this.selectedTenantIds().size === items.length;
  });

  protected readonly Math = Math;

  protected override async queryAsync(
    params: TableQueryParams,
  ): Promise<PagedResponse<TenantQuotaUsageDto>> {
    return this.api.invoke(getTenantQuotaUsages, {
      Page: params.page + 1,
      PageSize: params.size,
      OrderBy: formatSortField(params.sortField, params.sortOrder) || "-UsedThisWeek",
    });
  }

  protected toggleSelect(tenantId: string): void {
    this.selectedTenantIds.update((set) => {
      const next = new Set(set);
      if (next.has(tenantId)) {
        next.delete(tenantId);
      } else {
        next.add(tenantId);
      }
      return next;
    });
  }

  protected toggleSelectAll(): void {
    if (this.allSelected()) {
      this.selectedTenantIds.set(new Set());
    } else {
      this.selectedTenantIds.set(new Set(this.data().map((u) => u.tenantId!)));
    }
  }

  protected isSelected(tenantId: string): boolean {
    return this.selectedTenantIds().has(tenantId);
  }

  protected async resetSelected(): Promise<void> {
    const ids = Array.from(this.selectedTenantIds());
    if (ids.length === 0) return;

    this.isResetting.set(true);
    try {
      await this.api.invoke(resetTenantQuotas, { body: { tenantIds: ids } });
      this.toastService.showSuccess(`Quota reset for ${ids.length} tenant(s)`);
      this.selectedTenantIds.set(new Set());
      this.fetch({ page: 0, size: 10 });
    } catch {
      this.toastService.showError("Failed to reset quotas");
    } finally {
      this.isResetting.set(false);
    }
  }

  protected async resetAll(): Promise<void> {
    this.isResetting.set(true);
    try {
      await this.api.invoke(resetTenantQuotas, { body: { tenantIds: [] } });
      this.toastService.showSuccess("Quota reset for all tenants");
      this.selectedTenantIds.set(new Set());
      this.fetch({ page: 0, size: 10 });
    } catch {
      this.toastService.showError("Failed to reset quotas");
    } finally {
      this.isResetting.set(false);
    }
  }
}
