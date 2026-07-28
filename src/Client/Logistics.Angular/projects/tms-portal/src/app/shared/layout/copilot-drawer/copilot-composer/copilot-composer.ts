import { Component, input, output, signal } from "@angular/core";
import { UiButton, UiTextareaField } from "@logistics/shared/ui";

/**
 * Free-text message composer. Enter sends, Shift+Enter inserts a newline; while a turn runs
 * the send button becomes a stop button.
 */
@Component({
  selector: "app-copilot-composer",
  templateUrl: "./copilot-composer.html",
  imports: [UiButton, UiTextareaField],
})
export class CopilotComposer {
  public readonly isRunning = input(false);
  public readonly disabled = input(false);
  public readonly sendMessage = output<string>();
  public readonly stopTurn = output<void>();

  protected readonly text = signal("");

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.submit();
    }
  }

  protected submit(): void {
    if (this.isRunning()) return;
    const text = this.text().trim();
    if (!text || this.disabled()) return;
    this.sendMessage.emit(text);
    this.text.set("");
  }
}
