import {Component, OnInit} from '@angular/core';
import {NgIf, CurrencyPipe} from '@angular/common';
import {ChartModule} from 'primeng/chart';
import {SharedModule} from 'primeng/api';
import {SkeletonModule} from 'primeng/skeleton';
import {CardModule} from 'primeng/card';
import {MonthlyGrosses, OverallStats} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService} from '@core/services';
import {DateUtils, DistanceUtils} from '@shared/utils';


@Component({
  selector: 'app-main-dashboard',
  templateUrl: './main-dashboard.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    CardModule,
    NgIf,
    SkeletonModule,
    SharedModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
  ],
})
export class MainDashboardComponent implements OnInit {
  public loadingData: boolean;
  public loadingChart: boolean;
  public rpm: number;
  public overallStats?: OverallStats;
  public chartData: any;
  public chartOptions: any;

  constructor(private apiService: ApiService)
  {
    this.loadingData = false;
    this.loadingChart = false;
    this.rpm = 0;

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

  public ngOnInit(): void {
    this.fetchOverallStats();
    this.fetchMonthlyGrosses();
  }

  private fetchOverallStats() {
    this.loadingData = true;

    this.apiService.getOverallStats().subscribe((result) => {
      if (result.success && result.value) {
        const stats = result.value;
        this.overallStats = result.value;
        this.rpm = stats.totalIncome / DistanceUtils.metersTo(stats.totalDistance, 'mi');
      }

      this.loadingData = false;
    });
  }

  private fetchMonthlyGrosses() {
    this.loadingChart = true;
    const thisYear = DateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear).subscribe((result) => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.drawChart(monthlyGrosses);
      }

      this.loadingChart = false;
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
