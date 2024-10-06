import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {CardModule} from 'primeng/card';
import {ButtonModule} from 'primeng/button';
import {TooltipModule} from 'primeng/tooltip';
import {PagedIntervalQuery, TruckStatsDto} from '@/core/models';
import {ApiService} from '@/core/services';
import {DateUtils} from '@/core/utils';
import {DistanceUnitPipe} from '@/core/pipes';
import {RangeCalendarComponent} from '@/components';


@Component({
  selector: 'app-truck-stats-table',
  standalone: true,
  templateUrl: './truck-stats-table.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CurrencyPipe,
    TableModule,
    RouterLink,
    DatePipe,
    DistanceUnitPipe,
    CardModule,
    ButtonModule,
    RangeCalendarComponent,
    TooltipModule
  ],
})
export class TruckStatsTableComponent {
  public isLoading: boolean;
  public truckStats: TruckStatsDto[];
  public totalRecords: number;
  public startDate: Date;
  public endDate: Date;

  constructor(private apiService: ApiService) {
    this.isLoading = true;
    this.truckStats = [];
    this.totalRecords = 0;
    this.startDate = DateUtils.daysAgo(30);
    this.endDate = DateUtils.today();
  }

  reloadTable() {
    this.fetchTrucksStats({first: 0, rows: 10});
  }

  fetchTrucksStats(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    const query: PagedIntervalQuery = {
      startDate: this.startDate,
      endDate: this.endDate,
      orderBy: sortField,
      page: page,
      pageSize: rows
    }

    this.apiService.getTrucksStats(query).subscribe((result) => {
      if (result.success && result.data) {
        this.truckStats = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }
}
