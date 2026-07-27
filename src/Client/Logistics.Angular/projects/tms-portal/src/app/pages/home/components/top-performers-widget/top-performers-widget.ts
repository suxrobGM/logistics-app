import { Component, computed, inject, input } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import type { TopTruckDto } from "@logistics/shared/api";
import {
  Card,
  Divider,
  EmptyState,
  Icon,
  Progress,
  Skeleton,
  Stack,
  Typography,
  type IconName,
} from "@logistics/shared/ui";

interface RankIcon {
  name: IconName;
  /** Inline color value to inherit through the wrapper, since these tones aren't in IconColor enum. */
  color: string;
}

@Component({
  selector: "app-top-performers-widget",
  templateUrl: "./top-performers-widget.html",
  imports: [
    Card,
    CurrencyFormatPipe,
    Divider,
    EmptyState,
    Icon,
    Progress,
    RouterLink,
    Skeleton,
    Stack,
    Typography,
  ],
})
export class TopPerformersWidgetComponent {
  private readonly router = inject(Router);

  public readonly topTrucks = input<TopTruckDto[] | null>([]);
  public readonly isLoading = input<boolean>(false);

  protected readonly maxRevenue = computed(() => {
    const trucks = this.topTrucks();
    if (!trucks || trucks.length === 0) return 1;
    return Math.max(...trucks.map((t) => t.revenue ?? 0));
  });

  protected startFirstTruck(): void {
    void this.router.navigate(["/trucks/add"]);
  }

  protected getProgressValue(revenue: number | undefined): number {
    const max = this.maxRevenue();
    return max > 0 ? ((revenue ?? 0) / max) * 100 : 0;
  }

  protected getRankIcon(index: number): RankIcon {
    switch (index) {
      case 0:
        return { name: "star", color: "#eab308" }; // gold
      case 1:
        return { name: "star", color: "var(--text-muted)" }; // silver
      case 2:
        return { name: "star", color: "#b45309" }; // bronze
      default:
        return { name: "star", color: "var(--border-default)" };
    }
  }
}
