import { CurrencyPipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { LoadDto, LoadStatus } from "@logistics/shared/api";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-loads-summary-stats",
  templateUrl: "./loads-summary-stats.html",
  imports: [CardModule, CurrencyPipe],
})
export class LoadsSummaryStats {
  readonly loads = input.required<LoadDto[]>();
  readonly totalRecords = input.required<number>();
  readonly isLoading = input<boolean>(false);

  protected readonly statuses: LoadStatus[] = ["draft", "dispatched", "picked_up", "delivered", "cancelled"];

  protected readonly totalValue = computed(() => {
    return this.loads().reduce((sum, load) => sum + (load.deliveryCost ?? 0), 0);
  });

  protected readonly statusBreakdown = computed(() => {
    const counts: Record<LoadStatus, number> = {
      draft: 0,
      dispatched: 0,
      picked_up: 0,
      delivered: 0,
      cancelled: 0,
    };

    for (const load of this.loads()) {
      if (load.status && load.status in counts) {
        counts[load.status]++;
      }
    }

    return counts;
  });

  protected getStatusLabel(status: LoadStatus): string {
    const labels: Record<LoadStatus, string> = {
      draft: "Draft",
      dispatched: "Dispatched",
      picked_up: "Picked Up",
      delivered: "Delivered",
      cancelled: "Cancelled",
    };
    return labels[status] ?? status;
  }

  protected getStatusColor(status: LoadStatus): string {
    const colors: Record<LoadStatus, string> = {
      draft: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300",
      dispatched: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
      picked_up: "bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-300",
      delivered: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
      cancelled: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
    };
    return colors[status] ?? "";
  }
}
