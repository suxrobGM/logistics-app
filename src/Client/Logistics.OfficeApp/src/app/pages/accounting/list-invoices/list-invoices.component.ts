import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe} from '@angular/common';
import {RouterModule} from '@angular/router';
import {CardModule} from 'primeng/card';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {ButtonModule} from 'primeng/button';
import {TooltipModule} from 'primeng/tooltip';
import {Invoice} from '@core/models';
import {ApiService} from '@core/services';
import {PredefinedDateRanges} from '@core/helpers';
import {PaymentStatus} from '@core/enums';
import {PaymentStatusTagComponent} from '@shared/components';


@Component({
  selector: 'app-list-invoices',
  standalone: true,
  templateUrl: './list-invoices.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    PaymentStatusTagComponent,
  ],
})
export class ListInvoicesComponent {
  public paymentStatus = PaymentStatus;
  public invoices: Invoice[] = [];
  public isLoading = true;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {}

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    const past90days = PredefinedDateRanges.getPast90Days();

    this.apiService.getInvoices({
      orderBy: sortField,
      page: page,
      pageSize: rows,
      startDate: past90days.startDate,
      endDate: past90days.endDate
    }).subscribe((result) => {
      if (result.isSuccess && result.data) {
        this.invoices = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }
}
