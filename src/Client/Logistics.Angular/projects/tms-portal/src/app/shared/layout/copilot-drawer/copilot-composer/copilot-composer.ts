import { Component, computed, input, linkedSignal, output, signal, viewChild } from "@angular/core";
import { UiButton, UiTextareaField } from "@logistics/shared/ui";
import {
  COPILOT_COMMANDS,
  type CopilotCommand,
  type CopilotCommandAction,
} from "./copilot-commands";

/**
 * Enter sends, Shift+Enter newline; while a turn runs the send button becomes stop. A leading "/"
 * opens the command panel (Enter picks); unknown "/whatever" still sends as text.
 */
@Component({
  selector: "app-copilot-composer",
  templateUrl: "./copilot-composer.html",
  imports: [UiButton, UiTextareaField],
})
export class CopilotComposer {
  private readonly textarea = viewChild(UiTextareaField);

  public readonly isRunning = input(false);
  /** In-flight send: shows the button spinner and blocks a second submit. */
  public readonly disabled = input(false);
  /** Hard AI pause (budget block): dead textarea and button, no spinner. */
  public readonly blocked = input(false);
  public readonly sendMessage = output<string>();
  public readonly stopTurn = output<void>();
  public readonly commandSelected = output<CopilotCommandAction>();

  protected readonly text = signal("");

  /** Input value the panel was Escape-dismissed for; typing anything else re-opens it. */
  private readonly dismissedFor = signal<string | null>(null);

  protected readonly commandSuggestions = computed<readonly CopilotCommand[]>(() => {
    const raw = this.text().trim();
    if (!raw.startsWith("/") || raw.includes(" ") || this.dismissedFor() === raw) return [];
    const query = raw.slice(1).toLowerCase();
    return COPILOT_COMMANDS.filter((c) => c.name.startsWith(query));
  });

  /** Re-filtering resets the cursor, so it can never point past the narrowed list. */
  protected readonly activeIndex = linkedSignal<readonly CopilotCommand[], number>({
    source: this.commandSuggestions,
    computation: () => 0,
  });

  /** Focus target for the drawer's open-focus management. */
  public focus(): void {
    this.textarea()?.focus();
  }

  protected onKeydown(event: KeyboardEvent): void {
    const suggestions = this.commandSuggestions();
    if (suggestions.length > 0) {
      if (event.key === "ArrowDown") {
        event.preventDefault();
        this.activeIndex.set((this.activeIndex() + 1) % suggestions.length);
        return;
      }
      if (event.key === "ArrowUp") {
        event.preventDefault();
        this.activeIndex.set((this.activeIndex() - 1 + suggestions.length) % suggestions.length);
        return;
      }
      if (event.key === "Escape") {
        // Swallowed so the drawer's document-level Escape doesn't also close the drawer.
        event.stopPropagation();
        this.dismissedFor.set(this.text().trim());
        return;
      }
      if (event.key === "Enter" && !event.shiftKey) {
        event.preventDefault();
        this.runCommand(suggestions[this.activeIndex()]);
        return;
      }
    }

    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.submit();
    }
  }

  protected dismissSuggestions(): void {
    this.dismissedFor.set(this.text().trim());
  }

  protected runCommand(cmd: CopilotCommand): void {
    this.text.set("");
    this.commandSelected.emit(cmd.action);
  }

  protected submit(): void {
    if (this.isRunning() || this.blocked()) return;
    const text = this.text().trim();
    if (!text || this.disabled()) return;

    const exact = COPILOT_COMMANDS.find((c) => `/${c.name}` === text.toLowerCase());
    if (exact) {
      this.runCommand(exact);
      return;
    }

    this.sendMessage.emit(text);
    this.text.set("");
  }
}
