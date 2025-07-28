import {CommonModule, CurrencyPipe} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {CardModule} from "primeng/card";
import {SkeletonModule} from "primeng/skeleton";
import {ApiService} from "@/core/api";
import {DailyGrossesDto, MonthlyGrossesDto, TruckDto, TruckGeolocationDto} from "@/core/api/models";
import {BarChartDrawnEvent, GeolocationMap, GrossesBarchart} from "@/shared/components";
import {DistanceUnitPipe} from "@/shared/pipes";
import {LineChartDrawnEvent, TruckGrossesLinechartComponent} from "../components";

@Component({
  selector: "app-truck-details",
  templateUrl: "./truck-details.html",
  styleUrl: "./truck-details.css",
  imports: [
    CommonModule,
    CardModule,
    SkeletonModule,
    RouterLink,
    CurrencyPipe,
    DistanceUnitPipe,
    GeolocationMap,
    TruckGrossesLinechartComponent,
    GrossesBarchart,
  ],
})
export class TruckDetailsComponent implements OnInit {
  private readonly apiService = inject(ApiService);

  protected readonly id = input<string>();
  protected readonly isLoading = signal(false);
  protected readonly truck = signal<TruckDto | null>(null);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly monthlyGrosses = signal<MonthlyGrossesDto | null>(null);
  protected readonly rpmCurrent = signal(0);
  protected readonly rpmAllTime = signal(0);
  protected readonly truckLocations = signal<TruckGeolocationDto[]>([]);

  ngOnInit(): void {
    this.fetchTruck();
  }

  onLineChartDrawn(event: LineChartDrawnEvent): void {
    this.dailyGrosses.set(event.dailyGrosses);
    this.rpmCurrent.set(event.rpm);
  }

  onBarChartDrawn(event: BarChartDrawnEvent): void {
    this.monthlyGrosses.set(event.monthlyGrosses);
    this.rpmAllTime.set(event.rpm);
  }

  private fetchTruck(): void {
    const id = this.id();

    if (!id) {
      return;
    }

    this.isLoading.set(true);

    this.apiService.truckApi.getTruck(id).subscribe((result) => {
      if (result.success && result.data) {
        const truck = result.data;
        this.truck.set(truck);

        this.truckLocations.set([
          {
            latitude: truck.currentLocationLat,
            longitude: truck.currentLocationLong,
            truckId: truck.id,
            truckNumber: truck.number,
            driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
              .filter(Boolean)
              .join(", "),
          },
        ]);
      }

      this.isLoading.set(false);
    });
  }
}
