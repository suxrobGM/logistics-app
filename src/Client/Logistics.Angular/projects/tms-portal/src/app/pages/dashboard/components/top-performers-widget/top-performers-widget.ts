import { CurrencyPipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { TopTruckDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressBarModule } from "primeng/progressbar";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "app-top-performers-widget",
  templateUrl: "./top-performers-widget.html",
  imports: [CardModule, DividerModule, RouterLink, CurrencyPipe, SkeletonModule, ProgressBarModule],
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

  protected getRankIcon(index: number): string {
    switch (index) {
      case 0:
        return "pi pi-star-fill text-yellow-500";
      case 1:
        return "pi pi-star-fill text-gray-400";
      case 2:
        return "pi pi-star-fill text-amber-700";
      default:
        return "pi pi-star text-gray-300";
    }
  }
}
