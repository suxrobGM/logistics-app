import {Component, OnInit} from '@angular/core';
import {NgIf, CurrencyPipe, NgFor} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ChartModule} from 'primeng/chart';
import {SharedModule} from 'primeng/api';
import {SkeletonModule} from 'primeng/skeleton';
import {CardModule} from 'primeng/card';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {ButtonModule} from 'primeng/button';
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

  constructor(private apiService: ApiService) {
    this.isLoadingTruckStats = false;
    this.isLoadingOverallStats = false;
    this.isLoadingChart = false;
    this.rpm = 0;
    this.truckStats = [];
    this.totalTrucks = 0;

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

  fetchTrucksStats(event: TableLazyLoadEvent) {
    this.isLoadingTruckStats = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    const oneMonthAgo = DateUtils.daysAgo(30);
    const today = DateUtils.today();

    this.apiService.getTruckStats(oneMonthAgo, today, sortField, page, event.rows!).subscribe((result) => {
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
