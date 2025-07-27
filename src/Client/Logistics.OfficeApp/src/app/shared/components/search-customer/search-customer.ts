/* eslint-disable @typescript-eslint/no-empty-function */
import {CommonModule} from "@angular/common";
import {Component, forwardRef, inject, model, output, signal} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AutoCompleteModule, AutoCompleteSelectEvent} from "primeng/autocomplete";
import {ApiService} from "@/core/api";
import {CustomerDto} from "@/core/api/models";

@Component({
  selector: "app-search-customer",
  templateUrl: "./search-customer.html",
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
  private readonly apiService = inject(ApiService);

  protected readonly suggestedCustomers = signal<CustomerDto[]>([]);
  public readonly selectedCustomer = model<CustomerDto | null>(null);
  public readonly selectedCustomerChange = output<CustomerDto | null>();

  searchCustomer(event: {query: string}): void {
    this.apiService.getCustomers({search: event.query}).subscribe((result) => {
      if (result.data && result.data.length) {
        this.suggestedCustomers.set(result.data);
      }
    });
  }

  changeSelectedCustomer(event: AutoCompleteSelectEvent): void {
    this.selectedCustomerChange.emit(event.value);
    this.onChange(event.value);
  }

  //#region Implementation Reactive forms

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: CustomerDto | null): void {}
  private onTouched(): void {}

  writeValue(value: CustomerDto | null): void {
    this.selectedCustomer.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedCustomer.set(null);
    }
  }

  //#endregion
}
