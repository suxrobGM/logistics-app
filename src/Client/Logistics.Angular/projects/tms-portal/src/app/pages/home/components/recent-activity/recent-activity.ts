import { Component, computed, inject, input } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import {
  Card,
  Divider,
  EmptyState,
  Icon,
  Skeleton,
  Stack,
  Typography,
  type IconColor,
  type IconName,
} from "@logistics/shared/ui";

interface ActivityItem {
  loadId: string;
  loadNumber: number;
  action: string;
  timestamp: Date;
  icon: IconName;
  color: IconColor;
}

@Component({
  selector: "app-recent-activity",
  templateUrl: "./recent-activity.html",
  imports: [
    Card,
    Divider,
    EmptyState,
    Icon,
    RelativeTimePipe,
    RouterLink,
    Skeleton,
    Stack,
    Typography,
  ],
})
export class RecentActivityComponent {
  private readonly router = inject(Router);

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
          icon: "circle-check",
          color: "success",
        });
      } else if (load.pickedUpAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was picked up",
          timestamp: new Date(load.pickedUpAt),
          icon: "box",
          color: "warning",
        });
      } else if (load.dispatchedAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was dispatched",
          timestamp: new Date(load.dispatchedAt),
          icon: "send",
          color: "info",
        });
      } else if (load.cancelledAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was cancelled",
          timestamp: new Date(load.cancelledAt),
          icon: "circle-x",
          color: "danger",
        });
      } else if (load.createdAt) {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          action: "was created",
          timestamp: new Date(load.createdAt),
          icon: "circle-plus",
          color: "muted",
        });
      }
    }

    // Sort by timestamp, most recent first
    return items.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime()).slice(0, 5);
  });

  protected startFirstLoad(): void {
    void this.router.navigate(["/loads/add"]);
  }
}
