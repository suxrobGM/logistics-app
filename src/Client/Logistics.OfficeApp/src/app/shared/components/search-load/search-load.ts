/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, input, model, output, signal } from "@angular/core";
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { AutoCompleteModule, AutoCompleteSelectEvent } from "primeng/autocomplete";
import { ApiService } from "@/core/api";
import { LoadDto } from "@/core/api/models";
import { LoadStatusTag } from "../tags/load-status-tag/load-status-tag";

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
      useExisting: forwardRef(() => SearchLoadComponent),
      multi: true,
    },
  ],
})
export class SearchLoadComponent implements ControlValueAccessor {
  private readonly apiService = inject(ApiService);

  protected readonly suggestedLoads = signal<LoadDto[]>([]);

  public readonly filterActiveLoads = input(false);
  public readonly selectedLoad = model<LoadDto | null>(null);
  public readonly selectedLoadChange = output<LoadDto | null>();

  protected searchLoad(event: { query: string }): void {
    this.apiService.loadApi
      .getLoads({ search: event.query, onlyActiveLoads: this.filterActiveLoads() })
      .subscribe((result) => {
        if (!result.data) {
          return;
        }

        this.suggestedLoads.set(result.data);
      });
  }

  protected changeSelectedLoad(event: AutoCompleteSelectEvent): void {
    this.selectedLoadChange.emit(event.value);
    this.onChange(event.value);
  }

  private fetchLoad(loadId: string): void {
    if (!loadId) {
      this.selectedLoad.set(null);
      return;
    }

    this.apiService.loadApi.getLoad(loadId).subscribe((result) => {
      if (result.success && result.data) {
        this.selectedLoad.set(result.data);
      }
    });
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
