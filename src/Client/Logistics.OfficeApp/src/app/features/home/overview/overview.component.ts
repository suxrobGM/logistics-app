import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {DailyGrosses, Load} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService} from '@core/services';
import {DateUtils} from '@shared/utils';


@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class OverviewComponent implements OnInit {
  public todayGross: number;
  public weeklyGross: number;
  public weeklyDistance: number;
  public rpm: number;
  public loadingLoads: boolean;
  public loadingChart: boolean;
  public loads: Load[];
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private distanceUnit: DistanceUnitPipe)
  {
    this.loads = [];
    this.loadingLoads = false;
    this.loadingChart = false;
    this.todayGross = 0;
    this.weeklyGross = 0;
    this.weeklyDistance = 0;
    this.rpm = 0;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Daily Gross',
          data: [],
        },
      ],
    },

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };
  }

  public ngOnInit() {
    this.fetchLatestLoads();
    this.fetchLastTenDaysGross();
  }

  private fetchLatestLoads() {
    this.loadingLoads = true;

    this.apiService.getLoads('', '-dispatchedDate').subscribe((result) => {
      if (result.success && result.items) {
        this.loads = result.items;
      }

      this.loadingLoads = false;
    });
  }

  private fetchLastTenDaysGross() {
    this.loadingChart = true;
    const oneWeekAgo = DateUtils.daysAgo(7);

    this.apiService.getDailyGrosses(oneWeekAgo).subscribe((result) => {
      if (result.success && result.value) {
        const grosses = result.value;

        this.weeklyGross = grosses.totalIncome;
        this.weeklyDistance = grosses.totalDistance;
        this.rpm = this.weeklyGross / this.toMi(this.weeklyDistance);
        this.drawChart(grosses);
        this.calcTodayGross(grosses);
      }

      this.loadingChart = false;
    });
  }

  private drawChart(grosses: DailyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.dates.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
      data.push(i.income);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Daily Gross',
          data: data,
          fill: true,
          tension: 0.4,
          borderColor: '#405a83',
          backgroundColor: '#88a5d3',
        },
      ],
    };
  }

  private calcTodayGross(grosses: DailyGrosses) {
    const today = new Date();
    let totalGross = 0;

    grosses.dates
        .filter((i) => DateUtils.getDate(i.date) === today.getDate())
        .forEach((i) => totalGross += i.income);

    this.todayGross = totalGross;
  }

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
