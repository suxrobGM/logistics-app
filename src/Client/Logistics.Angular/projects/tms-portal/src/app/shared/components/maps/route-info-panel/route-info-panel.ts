import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { formatDistanceMiles, formatDuration, type RouteInfo, type SegmentInfo } from "../types";

/**
 * Displays route distance and estimated travel time.
 * Can show total route info and optionally per-segment breakdown.
 */
@Component({
  selector: "app-route-info-panel",
  templateUrl: "./route-info-panel.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RouteInfoPanel {
  /** Total distance in meters */
  public readonly distance = input<number>(0);

  /** Total duration in seconds */
  public readonly duration = input<number>(0);

  /** Per-segment breakdown (optional) */
  public readonly segments = input<SegmentInfo[]>([]);

  /** Show expanded segment details */
  public readonly showSegments = input(false);

  /** Compact mode for inline display */
  public readonly compact = input(false);

  /** Formatted route information */
  protected readonly routeInfo = computed<RouteInfo>(() => ({
    distance: this.distance(),
    duration: this.duration(),
    distanceMiles: formatDistanceMiles(this.distance()),
    durationFormatted: formatDuration(this.duration()),
  }));

  /** Format segment for display */
  protected formatSegment(seg: SegmentInfo): { miles: number; time: string } {
    return {
      miles: formatDistanceMiles(seg.distance),
      time: formatDuration(seg.duration),
    };
  }
}
