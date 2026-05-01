import { Component, computed, inject, input } from "@angular/core";
import { SkeletonModule } from "primeng/skeleton";
import { DashboardSettingsService } from "@/core/services";

/**
 * Layout-mirroring skeleton shown while initial home data is loading. Reads
 * the visible panel grid coordinates so the skeleton shapes match where the
 * real panels will land — replaces the blank-then-pop loading window.
 */
@Component({
  selector: "app-home-skeleton",
  templateUrl: "./home-skeleton.html",
  styleUrl: "./home-skeleton.css",
  imports: [SkeletonModule],
})
export class HomeSkeleton {
  private readonly dashboardSettings = inject(DashboardSettingsService);

  /** Single fixed row height used by the gridster (matches `fixedRowHeight: 120`). */
  public readonly rowHeight = input(120);

  protected readonly panels = computed(() =>
    this.dashboardSettings.visiblePanels().map((p) => ({
      id: p.id,
      gridColumn: `${p.x + 1} / span ${p.cols}`,
      gridRow: `${p.y + 1} / span ${p.rows}`,
    })),
  );

  protected readonly rowHeightPx = computed(() => `${this.rowHeight()}px`);
}
