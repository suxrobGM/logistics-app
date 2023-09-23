import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {NgIf, NgFor, CurrencyPipe, CommonModule} from '@angular/common';
import {SkeletonModule} from 'primeng/skeleton';
import {CardModule} from 'primeng/card';
import {DailyGrosses, MonthlyGrosses, Truck, TruckGeolocation} from '@core/models';
import {DistanceUnitPipe} from '@shared/pipes';
import {ApiService} from '@core/services';
import {GeolocationMapComponent, GrossesBarchartComponent, BarChartDrawnEvent} from '@shared/components';
import {AppConfig} from '@configs';
import {LineChartDrawnEvent, TruckGrossesLinechartComponent} from '../components';


@Component({
  selector: 'app-truck-details',
  templateUrl: './truck-details.component.html',
  styleUrls: ['./truck-details.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    SkeletonModule,
    RouterLink,
    CurrencyPipe,
    DistanceUnitPipe,
    GeolocationMapComponent,
    TruckGrossesLinechartComponent,
    GrossesBarchartComponent,
  ],
})
export class TruckDetailsComponent implements OnInit {
  public readonly accessToken: string;
  public id!: string;
  public isLoading: boolean;
  public truck?: Truck;
  public dailyGrosses?: DailyGrosses;
  public monthlyGrosses?: MonthlyGrosses;
  public rpmCurrent: number;
  public rpmAllTime: number;
  public truckLocations: TruckGeolocation[];

  constructor(
    private apiService: ApiService,
    private route: ActivatedRoute)
  {
    this.accessToken = AppConfig.mapboxToken;
    this.truckLocations = [];
    this.isLoading = false;
    this.rpmCurrent = 0;
    this.rpmAllTime = 0;
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    this.fetchTruck();
  }

  onLineChartDrawn(event: LineChartDrawnEvent) {
    this.dailyGrosses = event.dailyGrosses;
    this.rpmCurrent = event.rpm;
  }

  onBarChartDrawn(event: BarChartDrawnEvent) {
    this.monthlyGrosses = event.monthlyGrosses;
    this.rpmAllTime = event.rpm;
  }

  private fetchTruck() {
    this.isLoading = true;

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

      this.isLoading = false;
    });
  }
}
