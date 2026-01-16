import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, type PortalLoadDto, getPortalLoads } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";

@Component({
  selector: "cp-shipments-list",
  templateUrl: "./shipments-list.html",
  imports: [
    RouterLink,
    TableModule,
    ButtonModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    ProgressSpinnerModule,
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
      case "Delivered":
        return "success";
      case "PickedUp":
        return "info";
      case "Dispatched":
        return "warn";
      default:
        return "info";
    }
  }

  protected formatAddress(
    address: { city?: string | null; state?: string | null } | undefined,
  ): string {
    if (!address) return "-";
    const parts = [address.city, address.state].filter(Boolean);
    return parts.length > 0 ? parts.join(", ") : "-";
  }
}
