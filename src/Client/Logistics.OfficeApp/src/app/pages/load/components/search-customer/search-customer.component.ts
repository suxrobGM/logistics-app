import {CommonModule} from "@angular/common";
import {Component, Input, forwardRef, output} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ApiService} from "@/core/api";
import {CustomerDto} from "@/core/api/models";

@Component({
  selector: "app-search-customer",
  standalone: true,
  templateUrl: "./search-customer.component.html",
  imports: [CommonModule, AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchCustomerComponent),
      multi: true,
    },
  ],
})
export class SearchCustomerComponent implements ControlValueAccessor {
  private isDisabled = false;
  public suggestedCustomers: CustomerDto[] = [];

  @Input() selectedCustomer: CustomerDto | null = null;
  public readonly selectedCustomerChange = output<CustomerDto | null>();

  constructor(private readonly apiService: ApiService) {}

  searchCustomer(event: {query: string}) {
    this.apiService.getCustomers({search: event.query}).subscribe((result) => {
      if (result.data && result.data.length) {
        this.suggestedCustomers = result.data;
      }
    });
  }

  changeSelectedCustomer(event: AutoCompleteSelectEvent) {
    this.selectedCustomerChange.emit(event.value);
    this.onChange(event.value);
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: CustomerDto | null): void {}
  private onTouched(): void {}

  writeValue(value: CustomerDto | null): void {
    this.selectedCustomer = value;
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
