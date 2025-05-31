import {CommonModule, CurrencyPipe} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {CardModule} from "primeng/card";
import {SkeletonModule} from "primeng/skeleton";
import {ApiService} from "@/core/api";
import {DailyGrossesDto, MonthlyGrossesDto, TruckDto, TruckGeolocationDto} from "@/core/api/models";
import {environment} from "@/env";
import {
  BarChartDrawnEvent,
  GeolocationMapComponent,
  GrossesBarchartComponent,
} from "@/shared/components";
import {DistanceUnitPipe} from "@/shared/pipes";
import {LineChartDrawnEvent, TruckGrossesLinechartComponent} from "../components";

@Component({
  selector: "app-truck-details",
  templateUrl: "./truck-details.component.html",
  styleUrl: "./truck-details.component.css",
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
  public readonly accessToken = environment.mapboxToken;
  public id!: string;
  public isLoading = false;
  public truck?: TruckDto;
  public dailyGrosses?: DailyGrossesDto;
  public monthlyGrosses?: MonthlyGrossesDto;
  public rpmCurrent = 0;
  public rpmAllTime = 0;
  public truckLocations: TruckGeolocationDto[] = [];

  constructor(
    private readonly apiService: ApiService,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
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
      if (result.success && result.data) {
        this.truck = result.data;

        this.truckLocations = [
          {
            latitude: this.truck.currentLocationLat!,
            longitude: this.truck.currentLocationLong!,
            truckId: this.truck.id,
            truckNumber: this.truck.truckNumber,
            driversName: this.truck.drivers.map((driver) => driver.fullName).join(", "),
          },
        ];
      }

      this.isLoading = false;
    });
  }
}
