import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { Api, getTerminals, type TerminalDto } from "@logistics/shared/api";
import { AutoCompleteModule } from "primeng/autocomplete";

/**
 * Terminal search autocomplete.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 */
@Component({
  selector: "app-search-terminal",
  templateUrl: "./search-terminal.html",
  imports: [AutoCompleteModule, FormsModule],
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
