import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {CalendarModule} from 'primeng/calendar';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {CardModule} from 'primeng/card';
import {TruckStats} from '@core/models';
import {ApiService} from '@core/services';
import {DateUtils} from '@shared/utils';
import {DistanceUnitPipe} from '@shared/pipes';


@Component({
  selector: 'app-truck-stats-table',
  standalone: true,
  templateUrl: './truck-stats-table.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CurrencyPipe,
    TableModule,
    ButtonModule,
    RouterLink,
    CalendarModule,
    FormsModule,
    DatePipe,
    DistanceUnitPipe,
    CardModule,
  ],
})
export class TruckStatsTableComponent {
  public isLoading: boolean;
  public truckStats: TruckStats[];
  public totalRecords: number;
  public dates: Date[];
  public startDate: Date;
  public endDate: Date;
  public todayDate: Date;

  constructor(private apiService: ApiService) {
    this.isLoading = true;
    this.truckStats = [];
    this.totalRecords = 0;
    this.endDate = this.todayDate = DateUtils.today();
    this.startDate = DateUtils.daysAgo(30);
    this.dates = [this.startDate, this.endDate];
  }

  hasValidDates(): boolean {
    return this.dates && this.dates.length >= 2 && this.dates[0] != null && this.dates[1] != null;
  }

  reloadTable() {
    if (!this.hasValidDates()) {
      return;
    }

    this.startDate = this.dates[0];
    this.endDate = this.dates[1];
    this.fetchTrucksStats({first: 0, rows: 10});
  }

  fetchTrucksStats(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getTruckStats(this.startDate, this.endDate, sortField, page, event.rows!).subscribe((result) => {
      if (result.success && result.items) {
        this.truckStats = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isLoading = false;
    });
  }
}
