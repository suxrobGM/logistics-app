import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {TableLazyLoadEvent} from 'primeng/table';
import {PaymentMethod, getEnumDescription, PaymentMethodEnum, PaymentFor, PaymentForEnum} from '@core/enums';
import {Invoice} from '@core/models';
import {ApiService} from '@core/services';


@Component({
  selector: 'app-list-invoices',
  standalone: true,
  templateUrl: './list-invoices.component.html',
  styleUrls: ['./list-invoices.component.scss'],
  imports: [
    CommonModule,
  ],
})
export class ListInvoicesComponent {
  public invoices: Invoice[] = [];
  public isLoading = false;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {
  }

  // search(event: Event) {
  //   this.isLoading = true;
  //   const searchValue = (event.target as HTMLInputElement).value;

  //   this.apiService.getPayments({search: searchValue}).subscribe((result) => {
  //     if (result.isSuccess && result.data) {
  //       this.payments = result.data;
  //       this.totalRecords = result.totalItems;
  //     }

  //     this.isLoading = false;
  //   });
  // }

  // load(event: TableLazyLoadEvent) {
  //   this.isLoading = true;
  //   const first = event.first ?? 1;
  //   const rows = event.rows ?? 10;
  //   const page = first / rows + 1;
  //   const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

  //   this.apiService.getPayments({orderBy: sortField, page: page, pageSize: rows}).subscribe((result) => {
  //     if (result.isSuccess && result.data) {
  //       this.payments = result.data;
  //       this.totalRecords = result.totalItems;
  //     }

  //     this.isLoading = false;
  //   });
  // }

  // getPaymentMethodDesc(enumValue: PaymentMethod): string {
  //   return getEnumDescription(PaymentMethodEnum, enumValue);
  // }

  // getPaymentForDesc(enumValue: PaymentFor): string {
  //   return getEnumDescription(PaymentForEnum, enumValue);
  // }
}
