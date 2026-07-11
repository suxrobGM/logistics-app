import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getPortalLoads, type PortalLoadDto } from "@logistics/shared/api";
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
  selector: "cp-shipments-list",
  templateUrl: "./shipments-list.html",
  imports: [
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
export class ShipmentsList {
  private readonly api = inject(Api);

  protected readonly loads = signal<PortalLoadDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly isLoading = signal(true);
  protected readonly searchQuery = signal("");
  protected readonly tableFirst = signal(0);

  private currentPage = 1;
  private pageSize = 10;

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getPortalLoads, {
        Page: this.currentPage,
        PageSize: this.pageSize,
        Search: this.searchQuery() || undefined,
      });

      this.loads.set(result.items ?? []);
      this.totalRecords.set(result.pagination?.total ?? 0);
    } catch (error) {
      console.error("Failed to load shipments:", error);
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

  protected formatAddress(
    address: { city?: string | null; state?: string | null } | undefined,
  ): string {
    if (!address) return "-";
    const parts = [address.city, address.state].filter(Boolean);
    return parts.length > 0 ? parts.join(", ") : "-";
  }
}
