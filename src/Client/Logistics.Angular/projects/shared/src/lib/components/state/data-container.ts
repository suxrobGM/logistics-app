import { ChangeDetectionStrategy, Component, input, output } from "@angular/core";
import type { AppError } from "../../errors/error.types";
import { EmptyState } from "./empty-state";
import { ErrorState } from "./error-state";
import { LoadingSkeleton } from "./loading-skeleton";

/**
 * Container component that handles loading, error, empty, and data states.
 * Wraps content and shows appropriate state component based on inputs.
 */
@Component({
  selector: "ui-data-container",
  templateUrl: "./data-container.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [LoadingSkeleton, ErrorState, EmptyState],
})
export class DataContainer {
  /** Whether data is currently loading */
  public readonly loading = input(false);

  /** Error object if an error occurred */
  public readonly error = input<AppError | null>(null);

  /** Whether the data set is empty */
  public readonly isEmpty = input(false);

  // Skeleton configuration
  /** Skeleton variant to show while loading */
  public readonly skeletonVariant = input<"table" | "card" | "list">("table");

  /** Number of skeleton rows */
  public readonly skeletonRows = input(5);

  /** Optional loading text to display below skeleton */
  public readonly loadingText = input<string | null>(null);

  // Empty state configuration
  /** Title for empty state */
  public readonly emptyTitle = input("No data");

  /** Message for empty state */
  public readonly emptyMessage = input("No items to display.");

  /** Icon for empty state */
  public readonly emptyIcon = input("inbox");

  /** Action button label for empty state */
  public readonly emptyActionLabel = input<string | null>(null);

  // Events
  /** Emitted when user clicks retry on error state */
  public readonly retry = output<void>();

  /** Emitted when user clicks action button on empty state */
  public readonly emptyAction = output<void>();

  protected handleRetry(): void {
    this.retry.emit();
  }

  protected handleEmptyAction(): void {
    this.emptyAction.emit();
  }
}
