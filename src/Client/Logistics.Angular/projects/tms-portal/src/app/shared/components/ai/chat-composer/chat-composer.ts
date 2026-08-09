import { Component, computed, input, linkedSignal, output, signal, viewChild } from "@angular/core";
import { UiButton, UiTextareaField } from "@logistics/shared/ui";

/** A slash command the composer can suggest. `name` is compared without the leading "/". */
interface ChatComposerCommand {
  readonly name: string;
  readonly description: string;
}

/**
 * Shared chat composer for the copilot drawer and the AI dispatch page. The slash-command panel
 * only exists when `commands` is non-empty - the command vocabulary stays with the caller.
 */
@Component({
  selector: "app-chat-composer",
  templateUrl: "./chat-composer.html",
  imports: [UiButton, UiTextareaField],
  host: { class: "block" },
})
export class ChatComposer {
  private readonly textarea = viewChild(UiTextareaField);

  public readonly isRunning = input(false);
  /** In-flight send: shows the button spinner and blocks a second submit. */
  public readonly disabled = input(false);
  /** Hard block (e.g. AI budget pause): dead textarea and button, no spinner. */
  public readonly blocked = input(false);
  public readonly placeholder = input("Type a message...");
  public readonly commands = input<readonly ChatComposerCommand[]>([]);
  public readonly sendMessage = output<string>();
  public readonly stopTurn = output<void>();
  /** Emits the matched command's `name` - the caller maps it back to its own action. */
  public readonly commandSelected = output<string>();

  protected readonly text = signal("");

  /** Input value the panel was Escape-dismissed for; typing anything else re-opens it. */
  private readonly dismissedFor = signal<string | null>(null);

  protected readonly commandSuggestions = computed<readonly ChatComposerCommand[]>(() => {
    const raw = this.text().trim();
    if (!raw.startsWith("/") || raw.includes(" ") || this.dismissedFor() === raw) return [];
    const query = raw.slice(1).toLowerCase();
    return this.commands().filter((c) => c.name.startsWith(query));
  });

  /** Re-filtering resets the cursor, so it can never point past the narrowed list. */
  protected readonly activeIndex = linkedSignal<readonly ChatComposerCommand[], number>({
    source: this.commandSuggestions,
    computation: () => 0,
  });

  /** Focus target for the host's open-focus management. */
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
        // Swallowed so a document-level Escape on the host doesn't also close it.
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

  protected runCommand(cmd: ChatComposerCommand): void {
    this.text.set("");
    this.commandSelected.emit(cmd.name);
  }

  protected submit(): void {
    if (this.isRunning() || this.blocked()) return;
    const text = this.text().trim();
    if (!text || this.disabled()) return;

    const exact = this.commands().find((c) => `/${c.name}` === text.toLowerCase());
    if (exact) {
      this.runCommand(exact);
      return;
    }

    this.sendMessage.emit(text);
    this.text.set("");
  }
}
