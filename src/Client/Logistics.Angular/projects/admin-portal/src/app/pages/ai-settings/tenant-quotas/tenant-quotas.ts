import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { Api, resetTenantQuotas } from "@logistics/shared/api";
import {
  Badge,
  Card,
  Icon,
  Progress,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiSortHeader,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { TenantQuotasStore } from "./store";

@Component({
  selector: "adm-tenant-quotas",
  templateUrl: "./tenant-quotas.html",
  providers: [TenantQuotasStore],
  imports: [
    Badge,
    Card,
    CurrencyPipe,
    DatePipe,
    Icon,
    Progress,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiSortHeader,
  ],
})
export class TenantQuotas {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(TenantQuotasStore);

  protected readonly isResetting = signal(false);
  protected readonly selectedTenantIds = signal<Set<string>>(new Set());

  protected readonly allSelected = computed(() => {
    const items = this.store.data();
    return items.length > 0 && this.selectedTenantIds().size === items.length;
  });

  protected readonly Math = Math;

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
      this.selectedTenantIds.set(new Set(this.store.data().map((u) => u.tenantId!)));
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
      this.store.setPage(1);
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
      this.store.setPage(1);
    } catch {
      this.toastService.showError("Failed to reset quotas");
    } finally {
      this.isResetting.set(false);
    }
  }
}
