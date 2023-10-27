import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe} from '@angular/common';
import {RouterModule} from '@angular/router';
import {CardModule} from 'primeng/card';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {TooltipModule} from 'primeng/tooltip';
import {Payment} from '@core/models';
import {ApiService} from '@core/services';
import {LoadStatus, LoadStatusEnum, PaymentFor, PaymentForEnum, PaymentMethod, PaymentMethodEnum, getEnumDescription} from '@core/enums';


@Component({
  selector: 'app-list-payments',
  standalone: true,
  templateUrl: './list-payments.component.html',
  styleUrls: ['./list-payments.component.scss'],
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    TooltipModule,
    RouterModule,
    CurrencyPipe,
    DatePipe
  ],
})
export class ListPaymentsComponent {
  public payments: Payment[] = [];
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

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getPayments({orderBy: sortField, page: page, pageSize: rows}).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.payments = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  getPaymentMethodDesc(enumValue: PaymentMethod): string {
    return getEnumDescription(PaymentMethodEnum, enumValue);
  }

  getPaymentForDesc(enumValue: PaymentFor): string {
    return getEnumDescription(PaymentForEnum, enumValue);
  }
}
