import { ChangeDetectionStrategy, Component, input, output } from "@angular/core";
import type { AppError } from "@logistics/shared/errors";
import { EmptyState } from "./empty-state";
import { ErrorState } from "./error-state";
import { LoadingSkeleton } from "./loading-skeleton";

/**
 * Container component that handles loading, error, empty, and data states.
 * Wraps content and shows appropriate state component based on inputs.
 */
@Component({
  selector: "app-data-container",
  templateUrl: "./data-container.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [LoadingSkeleton, ErrorState, EmptyState],
})
export class DataContainer {
  /** Whether data is currently loading */
  readonly loading = input(false);

  /** Error object if an error occurred */
  readonly error = input<AppError | null>(null);

  /** Whether the data set is empty */
  readonly isEmpty = input(false);

  // Skeleton configuration
  /** Skeleton variant to show while loading */
  readonly skeletonVariant = input<"table" | "card" | "list">("table");

  /** Number of skeleton rows */
  readonly skeletonRows = input(5);

  // Empty state configuration
  /** Title for empty state */
  readonly emptyTitle = input("No data");

  /** Message for empty state */
  readonly emptyMessage = input("No items to display.");

  /** Icon for empty state */
  readonly emptyIcon = input("inbox");

  /** Action button label for empty state */
  readonly emptyActionLabel = input<string | null>(null);

  // Events
  /** Emitted when user clicks retry on error state */
  readonly retry = output<void>();

  /** Emitted when user clicks action button on empty state */
  readonly emptyAction = output<void>();

  protected handleRetry(): void {
    this.retry.emit();
  }

  protected handleEmptyAction(): void {
    this.emptyAction.emit();
  }
}
