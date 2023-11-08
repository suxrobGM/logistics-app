import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe, PercentPipe} from '@angular/common';
import {RouterModule} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {CardModule} from 'primeng/card';
import {InputTextModule} from 'primeng/inputtext';
import {TagModule} from 'primeng/tag';
import {ButtonModule} from 'primeng/button';
import {TooltipModule} from 'primeng/tooltip';
import {Payroll} from '@core/models';
import {
  PaymentMethod,
  PaymentMethodEnum,
  PaymentStatus,
  PaymentStatusEnum,
  SalaryType,
  SalaryTypeEnum,
} from '@core/enums';
import {ApiService} from '@core/services';


@Component({
  selector: 'app-list-payroll',
  standalone: true,
  templateUrl: './list-payroll.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CurrencyPipe,
    DatePipe,
    TableModule,
    CardModule,
    PercentPipe,
    InputTextModule,
    TagModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
  ],
})
export class ListPayrollComponent {
  public payrolls: Payroll[] = [];
  public isLoading = false;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getPayrolls({search: searchValue}).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.payrolls = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getPayrolls({
      orderBy: sortField, 
      page: page, 
      pageSize: rows,
    }).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.payrolls = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  isShareOfGrossSalary(salaryType: SalaryType): boolean {
    return salaryType === SalaryType.ShareOfGross;
  }

  isPendingPaymentStatus(paymentStatus: PaymentStatus): boolean {
    return paymentStatus === PaymentStatus.Pending;
  }

  getPaymentStatusTagSeverity(paymentStatus: PaymentStatus): string {
    return paymentStatus === PaymentStatus.Paid ? 'success' : 'warning';
  }

  getPaymentMethodDesc(enumValue?: PaymentMethod): string {
    if (enumValue == null) {
      return 'N/A';
    }

    return PaymentMethodEnum.getDescription(enumValue);
  }

  getPaymentStatusDesc(enumValue: PaymentStatus): string {
    return PaymentStatusEnum.getDescription(enumValue);
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return SalaryTypeEnum.getDescription(enumValue);
  }
}
