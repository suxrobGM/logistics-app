import { Component, type OnDestroy, computed, inject, input, output, signal } from "@angular/core";
import { Api, getTrucks } from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { TrackingService } from "@/core/services";
import { GeolocationMap } from "@/shared/components";

@Component({
  selector: "app-trucks-map",
  templateUrl: "./trucks-map.html",
  imports: [GeolocationMap],
})
export class TrucksMap implements OnDestroy {
  private readonly api = inject(Api);
  private readonly liveTrackingService = inject(TrackingService);

  protected readonly truckLocations = signal<TruckGeolocationDto[]>([]);

  /** Loading state for initial data fetch */
  protected readonly loading = signal(true);

  /** Count of trucks with valid location data */
  public readonly trucksWithLocation = computed(
    () => this.truckLocations().filter((t) => t.currentLocation).length,
  );

  /** Count of trucks without location data */
  public readonly trucksWithoutLocation = computed(
    () => this.truckLocations().filter((t) => !t.currentLocation).length,
  );

  public readonly width = input("100%");
  public readonly height = input("100%");

  /** Show map controls */
  public readonly showControls = input(true);

  /** Show layer toggle in controls */
  public readonly showLayerToggle = input(true);

  /** Emitted when a truck is selected on the map */
  public readonly truckSelect = output<TruckGeolocationDto>();

  constructor() {
    this.fetchTrucksData();
    this.connectToLiveTracking();
  }

  ngOnDestroy(): void {
    this.liveTrackingService.disconnect();
  }

  /** Retry fetching truck data after an error */
  public retry(): void {
    this.fetchTrucksData();
  }

  protected onTruckSelect(truck: TruckGeolocationDto): void {
    this.truckSelect.emit(truck);
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
    this.loading.set(true);

    try {
      const result = await this.api.invoke(getTrucks, { PageSize: 100 });

      if (!result.items) {
        this.truckLocations.set([]);
        return;
      }

      const truckLocations: TruckGeolocationDto[] = result.items.flatMap((truck) => {
        if (truck.currentLocation) {
          return [
            {
              latitude: truck.currentLocation.latitude,
              longitude: truck.currentLocation.longitude,
              truckId: truck.id,
              truckNumber: truck.number,
              driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
                .filter(Boolean)
                .join(", "),
              currentLocation: truck.currentLocation,
              currentAddress: truck.currentAddress,
            },
          ];
        }
        return [];
      });

      this.truckLocations.set(truckLocations);
    } finally {
      this.loading.set(false);
    }
  }
}
