import { Component, computed, input } from "@angular/core";
import type { TruckDto, TruckStatus } from "@logistics/shared/api";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-trucks-summary-stats",
  templateUrl: "./trucks-summary-stats.html",
  imports: [CardModule],
})
export class TrucksSummaryStats {
  readonly trucks = input.required<TruckDto[]>();
  readonly totalRecords = input.required<number>();
  readonly isLoading = input<boolean>(false);

  protected readonly statuses: TruckStatus[] = [
    "available",
    "en_route",
    "loading",
    "unloading",
    "maintenance",
    "out_of_service",
    "offline",
  ];

  protected readonly statusBreakdown = computed(() => {
    const counts: Record<TruckStatus, number> = {
      available: 0,
      en_route: 0,
      loading: 0,
      unloading: 0,
      maintenance: 0,
      out_of_service: 0,
      offline: 0,
    };

    for (const truck of this.trucks()) {
      if (truck.status && truck.status in counts) {
        counts[truck.status]++;
      }
    }

    return counts;
  });

  protected readonly availableCount = computed(() => this.statusBreakdown().available);

  protected readonly inUseCount = computed(() => {
    const breakdown = this.statusBreakdown();
    return breakdown.en_route + breakdown.loading + breakdown.unloading;
  });

  protected readonly unavailableCount = computed(() => {
    const breakdown = this.statusBreakdown();
    return breakdown.maintenance + breakdown.out_of_service + breakdown.offline;
  });

  protected getStatusLabel(status: TruckStatus): string {
    const labels: Record<TruckStatus, string> = {
      available: "Available",
      en_route: "En Route",
      loading: "Loading",
      unloading: "Unloading",
      maintenance: "Maintenance",
      out_of_service: "Out of Service",
      offline: "Offline",
    };
    return labels[status] ?? status;
  }

  protected getStatusColor(status: TruckStatus): string {
    const colors: Record<TruckStatus, string> = {
      available: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
      en_route: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
      loading: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
      unloading: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
      maintenance: "bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-300",
      out_of_service: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
      offline: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300",
    };
    return colors[status] ?? "";
  }
}
