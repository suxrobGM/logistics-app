import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, type Address, type LoadDto, dispatchLoad } from "@logistics/shared/api";
import { downloadBlobFile } from "@logistics/shared/utils";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import {
  BulkAssignDialog,
  LoadsBulkToolbar,
  type LoadsFilterState,
  LoadsFilterPanel,
  LoadsSummaryStats,
  LoadsTable,
} from "../components";
import { LoadsListStore } from "../store/loads-list.store";

@Component({
  selector: "app-loads-list",
  templateUrl: "./loads-list.html",
  providers: [LoadsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    LoadsFilterPanel,
    LoadsSummaryStats,
    LoadsBulkToolbar,
    LoadsTable,
    BulkAssignDialog,
  ],
})
export class LoadsListComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(LoadsListStore);

  // Bulk selection state
  protected readonly selectedLoads = signal<LoadDto[]>([]);
  protected readonly showBulkAssignDialog = signal(false);
  protected readonly isBulkProcessing = signal(false);

  protected addLoad(): void {
    this.router.navigate(["/loads/add"]);
  }

  protected onFiltersChanged(filters: LoadsFilterState): void {
    const apiFilters: Record<string, unknown> = {};

    if (filters.statuses.length > 0) {
      apiFilters["Statuses"] = filters.statuses;
    }
    if (filters.types.length > 0) {
      apiFilters["Types"] = filters.types;
    }
    if (filters.truck?.id) {
      apiFilters["TruckId"] = filters.truck.id;
    }
    if (filters.customer?.id) {
      apiFilters["CustomerId"] = filters.customer.id;
    }
    apiFilters["OnlyActiveLoads"] = filters.onlyActive;
    if (filters.dateRange?.length === 2) {
      apiFilters["StartDate"] = filters.dateRange[0].toISOString();
      apiFilters["EndDate"] = filters.dateRange[1].toISOString();
    }

    this.store.setFilters(apiFilters);
  }

  protected onSearchChanged(search: string): void {
    this.store.setSearch(search);
  }

  protected exportToCsv(): void {
    const loads = this.store.data();
    if (loads.length === 0) {
      this.toastService.showWarning("No loads to export");
      return;
    }

    const headers = [
      "Number",
      "Name",
      "Type",
      "Customer",
      "Origin",
      "Destination",
      "Status",
      "Distance (mi)",
      "Cost",
      "Truck",
      "Dispatcher",
    ];

    const rows = loads.map((load) => [
      load.number ?? "",
      load.name ?? "",
      load.type ?? "",
      load.customer?.name ?? "",
      this.formatAddress(load.originAddress),
      this.formatAddress(load.destinationAddress),
      load.status ?? "",
      load.distance ?? "",
      load.deliveryCost ?? "",
      load.assignedTruckNumber ?? "",
      load.assignedDispatcherName ?? "",
    ]);

    const csvContent = [headers, ...rows]
      .map((row) => row.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    downloadBlobFile(blob, `loads-export-${new Date().toISOString().split("T")[0]}.csv`);
    this.toastService.showSuccess(`Exported ${loads.length} load(s) to CSV`);
  }

  // Bulk action handlers
  protected openBulkAssignDialog(): void {
    this.showBulkAssignDialog.set(true);
  }

  protected onBulkAssigned(): void {
    this.clearSelection();
    this.store.retry();
  }

  protected onBulkDispatched(): void {
    this.clearSelection();
    this.store.retry();
  }

  protected onBulkDeleted(): void {
    this.clearSelection();
    this.store.retry();
  }

  protected clearSelection(): void {
    this.selectedLoads.set([]);
  }

  // Table action handlers
  protected onAssignLoad(load: LoadDto): void {
    this.selectedLoads.set([load]);
    this.showBulkAssignDialog.set(true);
  }

  protected async onDispatchLoad(load: LoadDto): Promise<void> {
    if (!load.id) return;

    try {
      await this.api.invoke(dispatchLoad, { id: load.id });
      this.toastService.showSuccess(`Load #${load.number} has been dispatched`);
      this.store.retry();
    } catch {
      this.toastService.showError("Failed to dispatch load");
    }
  }

  private formatAddress(address: Address | null | undefined): string {
    if (!address) return "";
    const parts = [address.line1, address.city, address.state].filter(Boolean);
    return parts.join(", ");
  }
}
