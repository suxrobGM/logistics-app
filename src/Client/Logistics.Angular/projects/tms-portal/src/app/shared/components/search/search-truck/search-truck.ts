/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { isEmptyGuid } from "@logistics/shared";
import { Api, type TruckDto, getTruckById, getTrucks } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

/**
 * Component for searching and selecting a truck.
 * This component uses an autocomplete input to allow users to search for trucks by name or number.
 * It accepts a truck ID or a TruckDto object as input and emits the selected truck.
 */
@Component({
  selector: "app-search-truck",
  templateUrl: "./search-truck.html",
  imports: [AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchTruck),
      multi: true,
    },
  ],
})
export class SearchTruck implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedTrucks = signal<TruckDto[]>([]);
  protected readonly disabled = signal<boolean>(false);

  public readonly selectedTruck = model<TruckDto | null>(null);
  public readonly selectedTruckChange = output<TruckDto | null>();

  protected async searchTruck(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getTrucks, { Search: event.query });

    if (!result.items) {
      return;
    }

    this.suggestedTrucks.set(result.items);
  }

  protected changeSelectedTruck(event: AutoCompleteSelectEvent): void {
    this.selectedTruckChange.emit(event.value);
    this.onChange(event.value);
  }

  private async fetchTruckById(id: string): Promise<void> {
    if (isEmptyGuid(id)) {
      this.selectedTruck.set(null);
      return;
    }

    const result = await this.api.invoke(getTruckById, { truckOrDriverId: id });
    if (result) {
      this.selectedTruck.set(result);
      this.onChange(result);
    }
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: TruckDto | null): void {}
  private onTouched(): void {}

  writeValue(value: TruckDto | string | null): void {
    if (typeof value === "string") {
      this.fetchTruckById(value);
      return;
    }

    this.selectedTruck.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  //#endregion
}
