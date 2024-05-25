import {Component, EventEmitter, Input, Output, forwardRef} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from '@angular/forms';
import {AutoCompleteModule, AutoCompleteSelectEvent} from 'primeng/autocomplete';
import {ApiService} from '@core/services';
import {Customer} from '@core/models';


@Component({
  selector: 'app-search-customer',
  standalone: true,
  templateUrl: './search-customer.component.html',
  imports: [
    CommonModule,
    AutoCompleteModule,
    FormsModule,
  ],
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
  public suggestedCustomers: Customer[] = [];

  @Input() selectedCustomer: Customer | null = null;
  @Output() selectedCustomerChange = new EventEmitter<Customer | null>();

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
  private onChange(value: Customer | null): void {}
  private onTouched(): void {}

  writeValue(value: Customer | null): void {
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
