import { Component, input, output } from "@angular/core";
import { UiButton } from "../../action/button/button";
import { Icon } from "../../icons/icon/icon";

/**
 * Error state component with retry functionality.
 */
@Component({
  selector: "ui-error-state",
  templateUrl: "./error-state.html",
  imports: [Icon, UiButton],
})
export class ErrorState {
  /** Title displayed above the error message */
  readonly title = input("Something went wrong");

  /** The error message to display */
  readonly message = input("An error occurred while loading data.");

  /** Whether to show the retry button */
  readonly retryable = input(true);

  /** Whether to show the logout button (for auth errors) */
  readonly showLogout = input(false);

  /** Emitted when the user clicks the retry button */
  readonly retry = output<void>();

  /** Emitted when the user clicks the logout button */
  readonly logout = output<void>();

  protected onRetry(): void {
    this.retry.emit();
  }

  protected onLogout(): void {
    this.logout.emit();
  }
}
