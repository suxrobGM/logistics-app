import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { SkeletonModule } from "primeng/skeleton";

/**
 * Configurable loading skeleton component with multiple layout variants.
 */
@Component({
  selector: "ui-loading-skeleton",
  templateUrl: "./loading-skeleton.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SkeletonModule],
})
export class LoadingSkeleton {
  /** The layout variant: 'table' for rows, 'card' for a block, 'list' for avatar+text */
  public readonly variant = input<"table" | "card" | "list">("table");

  /** Number of skeleton rows to show (for table and list variants) */
  public readonly rows = input(5);

  /** Height of each row (for table variant) */
  public readonly rowHeight = input("2.5rem");

  /** Width of the skeleton (for card variant) */
  public readonly width = input("100%");

  /** Height of the skeleton (for card variant) */
  public readonly height = input("10rem");

  /** Optional loading text to display below skeleton */
  public readonly loadingText = input<string | null>(null);

  protected readonly rowsArray = computed(() => Array.from({ length: this.rows() }));
}
