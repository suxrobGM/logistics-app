import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {NgIf, NgFor, CurrencyPipe} from '@angular/common';
import {ChartModule} from 'primeng/chart';
import {SharedModule} from 'primeng/api';
import {SkeletonModule} from 'primeng/skeleton';
import {CardModule} from 'primeng/card';
import {DailyGrosses, MonthlyGrosses, Truck, TruckGeolocation} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService} from '@core/services';
import {DateUtils, DistanceUtils} from '@shared/utils';
import {GeolocationMapComponent} from '@shared/components';
import {AppConfig} from '@configs';


@Component({
  selector: 'app-truck-details',
  templateUrl: './truck-details.component.html',
  styleUrls: ['./truck-details.component.scss'],
  standalone: true,
  imports: [
    CardModule,
    NgIf,
    SkeletonModule,
    NgFor,
    RouterLink,
    SharedModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
    GeolocationMapComponent,
  ],
})
export class TruckDetailsComponent implements OnInit {
  public readonly accessToken: string;
  public id!: string;
  public loadingData: boolean;
  public loadingBarChart: boolean;
  public loadingLineChart: boolean;
  public truck?: Truck;
  public dailyGrosses?: DailyGrosses;
  public monthlyGrosses?: MonthlyGrosses;
  public rpmCurrent: number;
  public rpmAllTime: number;
  public barChartData: any;
  public lineChartData: any;
  public chartOptions: any;
  public truckLocations: TruckGeolocation[];

  constructor(
    private apiService: ApiService,
    private route: ActivatedRoute)
  {
    this.accessToken = AppConfig.mapboxToken;
    this.truckLocations = [];
    this.loadingData = false;
    this.loadingLineChart = false;
    this.loadingBarChart = false;
    this.rpmCurrent = 0;
    this.rpmAllTime = 0;

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };

    this.barChartData = {
      labels: [],
      datasets: [
        {
          label: 'Monthly Gross',
          data: [],
        },
      ],
    };

    this.lineChartData = {
      labels: [],
      datasets: [
        {
          label: 'Daily Gross',
          data: [],
        },
      ],
    };
  }

  public ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    this.fetchTruck();
    this.fetchDailyGrosses();
    this.fetchMonthlyGrosses();
  }

  private fetchTruck() {
    this.loadingData = true;

    this.apiService.getTruck(this.id).subscribe((result) => {
      if (result.success && result.value) {
        this.truck = result.value;

        this.truckLocations = [{
          latitude: this.truck.currentLocationLat!,
          longitude: this.truck.currentLocationLong!,
          truckId: this.truck.id,
          truckNumber: this.truck.truckNumber,
          driversName: this.truck.drivers.map((driver) => driver.fullName).join(', '),
        }];
      }

      this.loadingData = false;
    });
  }

  private fetchDailyGrosses() {
    this.loadingLineChart = true;
    const oneMonthAgo = DateUtils.daysAgo(30);

    this.apiService.getDailyGrosses(oneMonthAgo, undefined, this.id).subscribe((result) => {
      if (result.success && result.value) {
        const dailyGrosses = result.value;
        this.dailyGrosses = result.value;
        this.rpmCurrent = dailyGrosses.totalIncome / DistanceUtils.metersTo(dailyGrosses.totalDistance, 'mi');

        this.drawLineChart(dailyGrosses);
      }

      this.loadingLineChart = false;
    });
  }

  private fetchMonthlyGrosses() {
    this.loadingBarChart = true;
    const thisYear = DateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear, undefined, this.id).subscribe((result) => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.monthlyGrosses = result.value;
        this.rpmAllTime = monthlyGrosses.totalIncome / DistanceUtils.metersTo(monthlyGrosses.totalDistance, 'mi');

        this.drawBarChart(monthlyGrosses);
      }

      this.loadingBarChart = false;
    });
  }

  private drawLineChart(grosses: DailyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.dates.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
      data.push(i.income);
    });

    this.lineChartData = {
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

  private drawBarChart(grosses: MonthlyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.months.forEach((i) => {
      labels.push(DateUtils.getMonthName(i.month));
      data.push(i.income);
    });

    this.barChartData = {
      labels: labels,
      datasets: [
        {
          label: 'Monthly Gross',
          data: data,
          fill: true,
          tension: 0.4,
          backgroundColor: '#EC407A',
        },
      ],
    };
  }
}
