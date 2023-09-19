import {Component, OnDestroy, OnInit, ViewEncapsulation} from '@angular/core';
import {DailyGrosses, GeolocationData, Load} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService, LiveTrackingService} from '@core/services';
import {DateUtils} from '@shared/utils';
import {ChartModule} from 'primeng/chart';
import {SkeletonModule} from 'primeng/skeleton';
import {NgIf, CurrencyPipe} from '@angular/common';
import {ButtonModule} from 'primeng/button';
import {TooltipModule} from 'primeng/tooltip';
import {RouterLink} from '@angular/router';
import {TableModule} from 'primeng/table';
import {GeolocationMapComponent} from '@shared/components';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    CardModule,
    SharedModule,
    GeolocationMapComponent,
    TableModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    NgIf,
    SkeletonModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
  ],
})
export class HomeComponent implements OnInit, OnDestroy {
  public todayGross: number;
  public weeklyGross: number;
  public weeklyDistance: number;
  public rpm: number;
  public loadingLoads: boolean;
  public loadingChart: boolean;
  public loads: Load[];
  public chartData: any;
  public chartOptions: any;
  public truksLocations: GeolocationData[];

  constructor(
    private apiService: ApiService,
    private liveTrackingService: LiveTrackingService,
    private distanceUnit: DistanceUnitPipe)
  {
    this.truksLocations = [];
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
    this.fetchActiveLoads();
    this.fetchLastTenDaysGross();
    this.fetchTrucksData();
    this.connectToLiveTracking();
  }

  public ngOnDestroy(): void {
    this.liveTrackingService.disconnect();
  }

  private connectToLiveTracking() {
    this.liveTrackingService.connect();

    this.liveTrackingService.onReceiveGeolocationData = (data: GeolocationData) => {
      const index = this.truksLocations.findIndex((loc) => loc.userId === data.userId);

      if (index !== -1) {
        this.truksLocations[index] = data;
      }
      else {
        this.truksLocations.push(data);
      }
    };
  }

  private fetchTrucksData() {
    this.apiService.getTrucks('', '', 1, 100).subscribe((result) => {
      if (!result.success) {
        return;
      }

      const truckLocations: GeolocationData[] = result.items!.flatMap((truck) => {
        if (truck.currentLocation) {
          return [{
            latitude: truck.currentLocationLat!,
            longitude: truck.currentLocationLong!,
            userId: truck.drivers[0].id,
            tenantId: '',
          }];
        }
        return [];
      });

      this.truksLocations = truckLocations;
    });
  }

  private fetchActiveLoads() {
    this.loadingLoads = true;

    this.apiService.getLoads('', true, '-dispatchedDate').subscribe((result) => {
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
