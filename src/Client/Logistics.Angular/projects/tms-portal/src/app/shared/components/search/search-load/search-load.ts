/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, input, model, output, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { Api, type LoadDto, getLoadById, getLoads } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { LoadStatusTag } from "../../tags/load-status-tag/load-status-tag";

/**
 * Component for searching and selecting loads.
 * It uses the AutoComplete component from PrimeNG to provide suggestions based on user input.
 */
@Component({
  selector: "app-search-load",
  templateUrl: "./search-load.html",
  imports: [AutoCompleteModule, FormsModule, LoadStatusTag],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchLoad),
      multi: true,
    },
  ],
})
export class SearchLoad implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedLoads = signal<LoadDto[]>([]);

  public readonly filterActiveLoads = input(false);
  public readonly selectedLoad = model<LoadDto | null>(null);
  public readonly selectedLoadChange = output<LoadDto | null>();

  protected async searchLoad(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getLoads, {
      Search: event.query,
      OnlyActiveLoads: this.filterActiveLoads(),
    });

    if (!result.items) {
      return;
    }

    this.suggestedLoads.set(result.items);
  }

  protected changeSelectedLoad(event: AutoCompleteSelectEvent): void {
    this.selectedLoadChange.emit(event.value);
    this.onChange(event.value);
  }

  private async fetchLoad(loadId: string): Promise<void> {
    if (!loadId) {
      this.selectedLoad.set(null);
      return;
    }

    const result = await this.api.invoke(getLoadById, { id: loadId });
    if (result) {
      this.selectedLoad.set(result);
    }
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: LoadDto | null): void {}
  private onTouched(): void {}

  writeValue(value: LoadDto | string | null): void {
    if (typeof value === "string") {
      this.fetchLoad(value);
      return;
    }

    this.selectedLoad.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedLoad.set(null);
    }
  }

  //#endregion
}
