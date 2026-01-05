import { ChangeDetectionStrategy, Component, computed, input, output } from "@angular/core";
import { ButtonModule } from "primeng/button";

/**
 * Empty state component for when there's no data to display.
 */
@Component({
  selector: "app-empty-state",
  templateUrl: "./empty-state.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ButtonModule],
})
export class EmptyState {
  /** Title displayed above the message */
  readonly title = input("No data found");

  /** Description message */
  readonly message = input("There are no items to display.");

  /** PrimeNG icon class (without 'pi pi-' prefix) */
  readonly icon = input("inbox");

  /** Label for the optional action button */
  readonly actionLabel = input<string | null>(null);

  /** Icon for the action button */
  readonly actionIcon = input("pi pi-plus");

  /** Emitted when the action button is clicked */
  readonly action = output<void>();

  protected readonly iconClass = computed(() => `pi pi-${this.icon()}`);

  protected onAction(): void {
    this.action.emit();
  }
}
