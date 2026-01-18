import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { SkeletonModule } from "primeng/skeleton";

/**
 * Configurable loading skeleton component with multiple layout variants.
 */
@Component({
  selector: "shared-loading-skeleton",
  templateUrl: "./loading-skeleton.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SkeletonModule],
})
export class LoadingSkeleton {
  /** The layout variant: 'table' for rows, 'card' for a block, 'list' for avatar+text */
  readonly variant = input<"table" | "card" | "list">("table");

  /** Number of skeleton rows to show (for table and list variants) */
  readonly rows = input(5);

  /** Height of each row (for table variant) */
  readonly rowHeight = input("2.5rem");

  /** Width of the skeleton (for card variant) */
  readonly width = input("100%");

  /** Height of the skeleton (for card variant) */
  readonly height = input("10rem");

  protected readonly rowsArray = computed(() => Array.from({ length: this.rows() }));
}
