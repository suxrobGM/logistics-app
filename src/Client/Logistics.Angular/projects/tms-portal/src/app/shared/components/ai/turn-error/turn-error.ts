import { Component, input, output } from "@angular/core";
import { Alert, Stack, UiButton } from "@logistics/shared/ui";

/** The retry banner shown when an agent turn fails, on any chat surface. */
@Component({
  selector: "app-turn-error",
  templateUrl: "./turn-error.html",
  imports: [Stack, Alert, UiButton],
})
export class TurnError {
  public readonly message = input<string | null>(null);
  public readonly fallback = input.required<string>();
  public readonly retry = output<void>();
}
