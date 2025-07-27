/* eslint-disable @typescript-eslint/no-empty-function */
import {CommonModule} from "@angular/common";
import {Component, forwardRef, inject, model, output, signal} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ApiService} from "@/core/api";

export interface SearchTruckData {
  driversName: string;
  truckId: string;
}

@Component({
  selector: "app-search-truck",
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

  protected readonly suggestedTrucks = signal<SearchTruckData[]>([]);
  public readonly selectedTruck = model<SearchTruckData | null>(null);
  public readonly selectedTruckChange = output<SearchTruckData | null>();

  searchTruck(event: {query: string}): void {
    this.apiService.getTruckDrivers({search: event.query}).subscribe((result) => {
      if (!result.data) {
        return;
      }

      this.suggestedTrucks.set(
        result.data.map((truckDriver) => ({
          truckId: truckDriver.truck.id,
          driversName: this.formatDriversName(
            truckDriver.truck.number,
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

  private formatDriversName(truckNumber: string, driversName: string[]): string {
    const formattedDriversName = driversName.join(",");
    return `${truckNumber} - ${formattedDriversName}`;
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: SearchTruckData | null): void {}
  private onTouched(): void {}

  writeValue(value: SearchTruckData | null): void {
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
