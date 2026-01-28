import { Component, input, output } from "@angular/core";
import { EmptyState, ErrorState } from "@logistics/shared";
import type { AppError } from "@logistics/shared/errors";

/**
 * Container component that wraps map components with loading, error, and empty state handling.
 * Uses an overlay approach for loading states to maintain map position.
 */
@Component({
  selector: "app-map-container",
  templateUrl: "./map-container.html",
  imports: [EmptyState, ErrorState],
})
export class MapContainer {
  /** Whether data is currently loading */
  public readonly loading = input(false);

  /** Error object if an error occurred */
  public readonly error = input<AppError | null>(null);

  /** Whether the data set is empty (no markers/waypoints) */
  public readonly isEmpty = input(false);

  /** Height of the map container */
  public readonly height = input("400px");

  // Empty state configuration
  /** Title for empty state */
  public readonly emptyTitle = input("No location data");

  /** Message for empty state */
  public readonly emptyMessage = input("Location data is not available.");

  /** Icon for empty state (PrimeNG icon name without 'pi pi-' prefix) */
  public readonly emptyIcon = input("map");

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
