import {Component, OnInit} from "@angular/core";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {CurrencyPipe, CommonModule} from "@angular/common";
import {SkeletonModule} from "primeng/skeleton";
import {CardModule} from "primeng/card";
import {DailyGrossesDto, MonthlyGrossesDto, TruckDto, TruckGeolocationDto} from "@/core/models";
import {DistanceUnitPipe} from "@/core/pipes";
import {ApiService} from "@/core/services";
import {GeolocationMapComponent, GrossesBarchartComponent, BarChartDrawnEvent} from "@/components";
import {GLOBAL_CONFIG} from "@/configs";
import {LineChartDrawnEvent, TruckGrossesLinechartComponent} from "../components";

@Component({
  selector: "app-truck-details",
  templateUrl: "./truck-details.component.html",
  styleUrls: ["./truck-details.component.scss"],
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
  public readonly accessToken = GLOBAL_CONFIG.mapboxToken;
  public id!: string;
  public isLoading = false;
  public truck?: TruckDto;
  public dailyGrosses?: DailyGrossesDto;
  public monthlyGrosses?: MonthlyGrossesDto;
  public rpmCurrent = 0;
  public rpmAllTime = 0;
  public truckLocations: TruckGeolocationDto[] = [];

  constructor(
    private apiService: ApiService,
    private route: ActivatedRoute
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
