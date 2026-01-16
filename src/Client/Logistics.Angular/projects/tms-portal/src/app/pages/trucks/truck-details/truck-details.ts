import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  type BarChartDrawnEvent,
  GeolocationMap,
  GrossesBarchart,
} from "@logistics/shared/components";
import { DistanceUnitPipe } from "@logistics/shared/pipes";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { Api, getTruckById } from "@/core/api";
import type {
  DailyGrossesDto,
  MonthlyGrossesDto,
  TruckDto,
  TruckGeolocationDto,
} from "@/core/api/models";
import { type LineChartDrawnEvent, TruckGrossesLinechartComponent } from "../components";

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
  private readonly api = inject(Api);

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

  private async fetchTruck(): Promise<void> {
    const id = this.id();

    if (!id) {
      return;
    }

    this.isLoading.set(true);

    const truck = await this.api.invoke(getTruckById, { truckOrDriverId: id });
    if (truck) {
      this.truck.set(truck);

      this.truckLocations.set([
        {
          currentLocation: truck.currentLocation,
          truckId: truck.id,
          truckNumber: truck.number,
          driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
            .filter(Boolean)
            .join(", "),
        },
      ]);
    }

    this.isLoading.set(false);
  }
}
