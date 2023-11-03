import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe} from '@angular/common';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {Payroll} from '@core/models';
import {
  PaymentMethod,
  getEnumDescription,
  PaymentMethodEnum,
  PaymentStatus,
  PaymentStatusEnum,
  PaymentFor,
  PaymentForEnum,
} from '@core/enums';
import {PredefinedDateRanges} from '@core/helpers';
import {ApiService} from '@core/services';
import { CardModule } from 'primeng/card';


@Component({
  selector: 'app-list-payroll',
  standalone: true,
  templateUrl: './list-payroll.component.html',
  styleUrls: ['./list-payroll.component.scss'],
  imports: [
    CommonModule,
    CurrencyPipe,
    DatePipe,
    TableModule,
    CardModule
  ],
})
export class ListPayrollComponent {
  public payrolls: Payroll[] = [];
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
    const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService.getPayrolls({
      orderBy: sortField, 
      page: page, 
      pageSize: rows,
      startDate: past90days.startDate,
      endDate: past90days.endDate
    }).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.payrolls = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  getPaymentMethodDesc(enumValue: PaymentMethod): string {
    return getEnumDescription(PaymentMethodEnum, enumValue);
  }

  getPaymentStatusDesc(enumValue: PaymentStatus): string {
    return getEnumDescription(PaymentStatusEnum, enumValue);
  }

  getPaymentForDesc(enumValue: PaymentFor): string {
    return getEnumDescription(PaymentForEnum, enumValue);
  }
}
