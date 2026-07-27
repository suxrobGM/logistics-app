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
} from "@logistics/shared/ui";

/** Bronze has no token of its own, so it is mixed off the same warning hue that stands in for gold. */
const RANK_COLORS = [
  "var(--warning)",
  "var(--text-muted)",
  "color-mix(in oklab, var(--warning) 55%, var(--text-muted))",
] as const;

const DEFAULT_RANK_COLOR = "var(--border-default)";

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

  /** Inline value, since these tones aren't in the IconColor enum - the wrapper inherits it. */
  protected rankColor(index: number): string {
    return RANK_COLORS[index] ?? DEFAULT_RANK_COLOR;
  }
}
