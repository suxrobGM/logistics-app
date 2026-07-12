import { Component, ElementRef, inject, input, model, output, signal } from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import { Api, getTerminals, type TerminalDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";

/**
 * Terminal search autocomplete.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 */
@Component({
  selector: "app-search-terminal",
  templateUrl: "./search-terminal.html",
  imports: [UiAutocompleteField],
})
export class SearchTerminal implements FormValueControl<TerminalDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedTerminals = signal<TerminalDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<TerminalDto | null>(null);

  /** Driven by the Reactive Forms / Signal Forms bridge. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  public readonly placeholder = model<string>("Type a terminal code or name");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected async searchTerminal(event: { query: string }): Promise<void> {
    const q = event.query?.trim() ?? "";
    this.lastQuery.set(q);

    if (q.length < 2) {
      this.suggestedTerminals.set([]);
      return;
    }

    try {
      const result = await this.api.invoke(getTerminals, { Search: q });
      this.suggestedTerminals.set(result.items ?? []);
    } catch {
      this.suggestedTerminals.set([]);
    }
  }
}
