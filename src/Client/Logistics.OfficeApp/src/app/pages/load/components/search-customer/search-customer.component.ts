import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ApiService} from '@core/services';
import {Customer} from '@core/models';


@Component({
  selector: 'app-search-customer',
  standalone: true,
  templateUrl: './search-customer.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    AutoCompleteModule,
  ],
})
export class SearchCustomerComponent {
  public suggestedCustomers: Customer[] = [];

  @Input() selectedCustomer: Customer | null = null;
  @Output() selectedCustomerChange = new EventEmitter<Customer>();

  constructor(private readonly apiService: ApiService){
  }

  searchCustomer(event: {query: string}) {
    this.apiService.getCustomers({search: event.query}).subscribe((result) => {
      if (result.data && result.data.length) {
        this.suggestedCustomers = result.data;
      }
    });
  }

  changeSelectedCustomer(event: Customer) {
    this.selectedCustomerChange.emit(event);
  }
}
