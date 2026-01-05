/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { AutoCompleteModule, AutoCompleteSelectEvent } from "primeng/autocomplete";
import { Api, TruckDto, getTruckById$Json, getTrucks$Json } from "@/core/api";

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
      useExisting: forwardRef(() => SearchTruckComponent),
      multi: true,
    },
  ],
})
export class SearchTruckComponent implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedTrucks = signal<TruckDto[]>([]);
  protected readonly disabled = signal<boolean>(false);

  public readonly selectedTruck = model<TruckDto | null>(null);
  public readonly selectedTruckChange = output<TruckDto | null>();

  protected async searchTruck(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getTrucks$Json, { Search: event.query });

    if (!result.data) {
      return;
    }

    this.suggestedTrucks.set(result.data);
  }

  protected changeSelectedTruck(event: AutoCompleteSelectEvent): void {
    this.selectedTruckChange.emit(event.value);
    this.onChange(event.value);
  }

  private async fetchTruckById(id: string): Promise<void> {
    if (!id) {
      this.selectedTruck.set(null);
      return;
    }

    const result = await this.api.invoke(getTruckById$Json, { truckOrDriverId: id });
    if (result.success && result.data) {
      this.selectedTruck.set(result.data);
      this.onChange(result.data);
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
