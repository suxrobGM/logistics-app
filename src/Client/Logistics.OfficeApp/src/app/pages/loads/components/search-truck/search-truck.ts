/* eslint-disable @typescript-eslint/no-empty-function */
import {CommonModule} from "@angular/common";
import {Component, forwardRef, inject, model, output, signal} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ApiService} from "@/core/api";
import {TruckData, TruckHelper} from "../../shared";

@Component({
  selector: "app-search-truck",
  standalone: true,
  templateUrl: "./search-truck.html",
  imports: [CommonModule, AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchTruckComponent),
      multi: true,
    },
  ],
})
export class SearchTruckComponent implements ControlValueAccessor {
  private readonly apiService = inject(ApiService);

  protected readonly suggestedTrucks = signal<TruckData[]>([]);
  public readonly selectedTruck = model<TruckData | null>(null);
  public readonly selectedTruckChange = output<TruckData | null>();

  searchTruck(event: {query: string}): void {
    this.apiService.getTruckDrivers({search: event.query}).subscribe((result) => {
      if (!result.data) {
        return;
      }

      this.suggestedTrucks.set(
        result.data.map((truckDriver) => ({
          truckId: truckDriver.truck.id,
          driversName: TruckHelper.formatDriversName(
            truckDriver.truck.truckNumber,
            truckDriver.drivers.map((i) => i.fullName)
          ),
        }))
      );
    });
  }

  changeSelectedTruck(event: AutoCompleteSelectEvent): void {
    this.selectedTruckChange.emit(event.value);
    this.onChange(event.value);
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: TruckData | null): void {}
  private onTouched(): void {}

  writeValue(value: TruckData | null): void {
    this.selectedTruck.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedTruck.set(null);
    }
  }

  //#endregion
}
