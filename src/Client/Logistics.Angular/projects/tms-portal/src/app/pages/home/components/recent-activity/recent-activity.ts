import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

interface ActivityItem {
  loadId: string;
  loadNumber: number;
  action: string;
  timestamp: Date;
  icon: string;
  iconClass: string;
}

@Component({
  selector: "app-recent-activity",
  templateUrl: "./recent-activity.html",
  imports: [CardModule, DividerModule, SkeletonModule, RouterLink, RelativeTimePipe],
})
export class RecentActivityComponent {
  readonly loads = input<LoadDto[]>([]);
  readonly isLoading = input(false);

  protected readonly activities = computed<ActivityItem[]>(() => {
    const items: ActivityItem[] = [];

    for (const load of this.loads()) {
      if (!load.id || !load.number) continue;

      // Add activity based on most recent status change
      if (load.deliveredAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was delivered",
          timestamp: new Date(load.deliveredAt),
          icon: "pi-check-circle",
          iconClass: "text-emerald-600 dark:text-emerald-400",
        });
      } else if (load.pickedUpAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was picked up",
          timestamp: new Date(load.pickedUpAt),
          icon: "pi-box",
          iconClass: "text-amber-600 dark:text-amber-400",
        });
      } else if (load.dispatchedAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was dispatched",
          timestamp: new Date(load.dispatchedAt),
          icon: "pi-send",
          iconClass: "text-blue-600 dark:text-blue-400",
        });
      } else if (load.cancelledAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was cancelled",
          timestamp: new Date(load.cancelledAt),
          icon: "pi-times-circle",
          iconClass: "text-red-600 dark:text-red-400",
        });
      } else if (load.createdAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was created",
          timestamp: new Date(load.createdAt),
          icon: "pi-plus-circle",
          iconClass: "text-gray-600 dark:text-gray-400",
        });
      }
    }

    // Sort by timestamp, most recent first
    return items.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime()).slice(0, 5);
  });
}
