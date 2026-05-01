import { Component, computed, input } from "@angular/core";
import type { TripDto, TripStatus } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";

@Component({
  selector: "app-trips-summary-stats",
  templateUrl: "./trips-summary-stats.html",
  imports: [CurrencyFormatPipe],
})
export class TripsSummaryStats {
  readonly trips = input.required<TripDto[]>();
  readonly totalRecords = input.required<number>();
  readonly isLoading = input<boolean>(false);

  protected readonly statuses: TripStatus[] = [
    "draft",
    "dispatched",
    "in_transit",
    "completed",
    "cancelled",
  ];

  protected readonly totalRevenue = computed(() =>
    this.trips().reduce((sum, trip) => sum + (trip.totalRevenue ?? 0), 0),
  );

  protected readonly statusBreakdown = computed(() => {
    const counts: Record<TripStatus, number> = {
      draft: 0,
      dispatched: 0,
      in_transit: 0,
      completed: 0,
      cancelled: 0,
    };

    for (const trip of this.trips()) {
      if (trip.status && trip.status in counts) {
        counts[trip.status]++;
      }
    }

    return counts;
  });

  protected getStatusLabel(status: TripStatus): string {
    const labels: Record<TripStatus, string> = {
      draft: "Draft",
      dispatched: "Dispatched",
      in_transit: "In Transit",
      completed: "Completed",
      cancelled: "Cancelled",
    };
    return labels[status] ?? status;
  }

  protected getStatusColor(status: TripStatus): string {
    const colors: Record<TripStatus, string> = {
      draft: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300",
      dispatched: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
      in_transit: "bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-300",
      completed: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
      cancelled: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
    };
    return colors[status] ?? "";
  }
}
