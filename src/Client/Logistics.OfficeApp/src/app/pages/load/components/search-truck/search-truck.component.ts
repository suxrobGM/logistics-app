import {CommonModule} from "@angular/common";
import {Component, Input, forwardRef, output} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ApiService} from "@/core/api";
import {TruckData, TruckHelper} from "../../shared";

@Component({
  selector: "app-search-truck",
  standalone: true,
  templateUrl: "./search-truck.component.html",
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
  private isDisabled = false;
  public suggestedTrucks: TruckData[] = [];

  @Input() selectedTruck: TruckData | null = null;
  readonly selectedTruckChange = output<TruckData>();

  constructor(private readonly apiService: ApiService) {}

  searchTruck(event: {query: string}) {
    this.apiService.getTruckDrivers({search: event.query}).subscribe((result) => {
      if (!result.data) {
        return;
      }

      this.suggestedTrucks = result.data.map((truckDriver) => ({
        truckId: truckDriver.truck.id,
        driversName: TruckHelper.formatDriversName(
          truckDriver.truck.truckNumber,
          truckDriver.drivers.map((i) => i.fullName)
        ),
      }));
    });
  }

  changeSelectedTruck(event: AutoCompleteSelectEvent) {
    this.selectedTruckChange.emit(event.value);
    this.onChange(event.value);
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: TruckData | null): void {}
  private onTouched(): void {}

  writeValue(value: TruckData | null): void {
    this.selectedTruck = value;
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  //#endregion
}
