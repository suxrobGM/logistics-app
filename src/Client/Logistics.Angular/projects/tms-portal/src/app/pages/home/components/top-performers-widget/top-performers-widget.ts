import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import type { TopTruckDto } from "@logistics/shared/api";
import { Icon, Stack, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressBarModule } from "primeng/progressbar";
import { SkeletonModule } from "primeng/skeleton";

interface RankIcon {
  name: string;
  /** Inline color value to inherit through the wrapper, since these tones aren't in IconColor enum. */
  color: string;
}

@Component({
  selector: "app-top-performers-widget",
  templateUrl: "./top-performers-widget.html",
  imports: [
    CardModule,
    DividerModule,
    RouterLink,
    SkeletonModule,
    ProgressBarModule,
    CurrencyFormatPipe,
    Icon,
    Stack,
    Typography,
  ],
})
export class TopPerformersWidgetComponent {
  public readonly topTrucks = input<TopTruckDto[] | null>([]);
  public readonly isLoading = input<boolean>(false);

  protected readonly maxRevenue = computed(() => {
    const trucks = this.topTrucks();
    if (!trucks || trucks.length === 0) return 1;
    return Math.max(...trucks.map((t) => t.revenue ?? 0));
  });

  protected getProgressValue(revenue: number | undefined): number {
    const max = this.maxRevenue();
    return max > 0 ? ((revenue ?? 0) / max) * 100 : 0;
  }

  protected getRankIcon(index: number): RankIcon {
    switch (index) {
      case 0:
        return { name: "star-fill", color: "#eab308" }; // gold
      case 1:
        return { name: "star-fill", color: "var(--text-muted)" }; // silver
      case 2:
        return { name: "star-fill", color: "#b45309" }; // bronze
      default:
        return { name: "star", color: "var(--border-default)" };
    }
  }
}
