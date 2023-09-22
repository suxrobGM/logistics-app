import {Component, OnInit} from '@angular/core';
import {NgIf, CurrencyPipe, NgFor, DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {ChartModule} from 'primeng/chart';
import {SharedModule} from 'primeng/api';
import {SkeletonModule} from 'primeng/skeleton';
import {CardModule} from 'primeng/card';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {ButtonModule} from 'primeng/button';
import {CalendarModule} from 'primeng/calendar';
import {MonthlyGrosses, OverallStats, TruckStats} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService} from '@core/services';
import {DateUtils, DistanceUtils} from '@shared/utils';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    CardModule,
    NgIf,
    NgFor,
    SkeletonModule,
    SharedModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
    TableModule,
    ButtonModule,
    RouterLink,
    CalendarModule,
    FormsModule,
    DatePipe,
  ],
})
export class DashboardComponent implements OnInit {
  public isLoadingTruckStats: boolean;
  public isLoadingOverallStats: boolean;
  public isLoadingChart: boolean;
  public rpm: number;
  public overallStats?: OverallStats;
  public chartData: any;
  public chartOptions: any;
  public truckStats: TruckStats[];
  public totalTrucks: number;
  public truckStatsDates: Date[];
  public startDate: Date;
  public endDate: Date;
  public todayDate: Date;

  constructor(private apiService: ApiService) {
    this.isLoadingTruckStats = true;
    this.isLoadingOverallStats = false;
    this.isLoadingChart = false;
    this.rpm = 0;
    this.truckStats = [];
    this.totalTrucks = 0;
    this.endDate = this.todayDate = DateUtils.today();
    this.startDate = DateUtils.daysAgo(30);
    this.truckStatsDates = [this.startDate, this.endDate];

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Monthly Gross',
          data: [],
        },
      ],
    };

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };
  }

  ngOnInit(): void {
    this.fetchOverallStats();
    this.fetchMonthlyGrosses();
  }

  reloadTable() {
    if (this.truckStatsDates.length < 2) {
      return;
    }

    this.startDate = this.truckStatsDates[0];
    this.endDate = this.truckStatsDates[1];
    this.fetchTrucksStats({first: 0, rows: 10});
  }

  fetchTrucksStats(event: TableLazyLoadEvent) {
    this.isLoadingTruckStats = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getTruckStats(this.startDate, this.endDate, sortField, page, event.rows!).subscribe((result) => {
      if (result.success && result.items) {
        this.truckStats = result.items;
        this.totalTrucks = result.itemsCount!;
      }

      this.isLoadingTruckStats = false;
    });
  }

  private fetchOverallStats() {
    this.isLoadingOverallStats = true;

    this.apiService.getOverallStats().subscribe((result) => {
      if (result.success && result.value) {
        const stats = result.value;
        this.overallStats = result.value;
        this.rpm = stats.totalIncome / DistanceUtils.metersTo(stats.totalDistance, 'mi');
      }

      this.isLoadingOverallStats = false;
    });
  }

  private fetchMonthlyGrosses() {
    this.isLoadingChart = true;
    const thisYear = DateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear).subscribe((result) => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.drawChart(monthlyGrosses);
      }

      this.isLoadingChart = false;
    });
  }

  private drawChart(grosses: MonthlyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.months.forEach((i) => {
      labels.push(DateUtils.getMonthName(i.month));
      data.push(i.income);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Monthly Gross',
          data: data,
          fill: true,
          tension: 0.4,
          backgroundColor: '#88a5d3',
        },
      ],
    };
  }
}
