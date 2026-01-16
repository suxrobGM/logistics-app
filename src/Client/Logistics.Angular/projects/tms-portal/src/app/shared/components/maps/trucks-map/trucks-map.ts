import { Component, type OnDestroy, inject, input, signal } from "@angular/core";
import { GeolocationMap } from "@logistics/shared/components";
import { Api, getTrucks } from "@/core/api";
import type { TruckGeolocationDto } from "@/core/api/models";
import { LiveTrackingService } from "@/core/services";

@Component({
  selector: "app-trucks-map",
  templateUrl: "./trucks-map.html",
  imports: [GeolocationMap],
})
export class TrucksMap implements OnDestroy {
  private readonly api = inject(Api);
  private readonly liveTrackingService = inject(LiveTrackingService);

  protected readonly truckLocations = signal<TruckGeolocationDto[]>([]);
  public readonly width = input("100%");
  public readonly height = input("100%");

  constructor() {
    this.fetchTrucksData();
    this.connectToLiveTracking();
  }

  ngOnDestroy(): void {
    this.liveTrackingService.disconnect();
  }

  private connectToLiveTracking(): void {
    this.liveTrackingService.connect();

    this.liveTrackingService.onReceiveGeolocationData = (data: TruckGeolocationDto) => {
      const index = this.truckLocations().findIndex((loc) => loc.truckId === data.truckId);

      if (index !== -1) {
        this.truckLocations.update((prev) => {
          const updatedLocations = [...prev];
          updatedLocations[index] = data;
          return updatedLocations;
        });
      } else {
        this.truckLocations.update((prev) => [...prev, data]);
      }
    };
  }

  private async fetchTrucksData(): Promise<void> {
    const result = await this.api.invoke(getTrucks, { PageSize: 100 });

    if (!result.items) {
      return;
    }

    const truckLocations: TruckGeolocationDto[] = result.items.flatMap((truck) => {
      if (truck.currentLocation) {
        return [
          {
            latitude: truck.currentLocation.latitude,
            longitude: truck.currentLocation.longitude,
            truckId: truck.id ?? undefined,
            truckNumber: truck.number ?? undefined,
            driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
              .filter(Boolean)
              .join(", "),
          },
        ];
      }
      return [];
    });

    this.truckLocations.set(truckLocations);
  }
}
