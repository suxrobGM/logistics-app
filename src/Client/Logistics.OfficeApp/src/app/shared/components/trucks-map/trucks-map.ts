import {CommonModule} from "@angular/common";
import {Component, OnDestroy, inject, input, signal} from "@angular/core";
import {ApiService} from "@/core/api";
import {TruckGeolocationDto} from "@/core/api/models";
import {LiveTrackingService} from "@/core/services";
import {GeolocationMap} from "@/shared/components";

@Component({
  selector: "app-trucks-map",
  templateUrl: "./trucks-map.html",
  imports: [CommonModule, GeolocationMap],
})
export class TrucksMap implements OnDestroy {
  private readonly apiService = inject(ApiService);
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

  private fetchTrucksData(): void {
    this.apiService.getTrucks({pageSize: 100}).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      const truckLocations: TruckGeolocationDto[] = result.data.flatMap((truck) => {
        if (truck.currentLocation) {
          return [
            {
              latitude: truck.currentLocationLat!,
              longitude: truck.currentLocationLong!,
              truckId: truck.id,
              truckNumber: truck.truckNumber,
              driversName: truck.drivers.map((driver) => driver.fullName).join(", "),
            },
          ];
        }
        return [];
      });

      this.truckLocations.set(truckLocations);
    });
  }
}
