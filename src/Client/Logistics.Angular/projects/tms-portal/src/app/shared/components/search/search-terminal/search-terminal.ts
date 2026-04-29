/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { FormsModule, NG_VALUE_ACCESSOR, type ControlValueAccessor } from "@angular/forms";
import { Api, getTerminals, type TerminalDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

@Component({
  selector: "app-search-terminal",
  templateUrl: "./search-terminal.html",
  imports: [AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchTerminal),
      multi: true,
    },
  ],
})
export class SearchTerminal implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedTerminals = signal<TerminalDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  public readonly selectedTerminal = model<TerminalDto | null>(null);
  public readonly selectedTerminalChange = output<TerminalDto | null>();
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

  protected changeSelectedTerminal(event: AutoCompleteSelectEvent): void {
    this.selectedTerminalChange.emit(event.value);
    this.onChange(event.value);
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: TerminalDto | null): void {}
  private onTouched(): void {}

  writeValue(value: TerminalDto | null): void {
    this.selectedTerminal.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedTerminal.set(null);
    }
  }
}
