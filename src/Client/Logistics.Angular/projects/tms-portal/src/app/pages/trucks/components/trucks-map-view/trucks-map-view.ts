import { Component, computed, input } from "@angular/core";
import type { TruckDto } from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { GeolocationMap } from "@/shared/components";

@Component({
  selector: "app-trucks-map-view",
  templateUrl: "./trucks-map-view.html",
  imports: [GeolocationMap, ButtonModule],
})
export class TrucksMapView {
  public readonly trucks = input<TruckDto[]>([]);
  public readonly isLoading = input(false);

  protected readonly truckLocations = computed<TruckGeolocationDto[]>(() => {
    return this.trucks()
      .filter((truck) => truck.currentLocation?.latitude && truck.currentLocation?.longitude)
      .map((truck) => ({
        truckId: truck.id,
        truckNumber: truck.number,
        driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
          .filter(Boolean)
          .join(", "),
        currentLocation: truck.currentLocation,
        currentAddress: truck.currentAddress,
      }));
  });

  protected readonly trucksWithLocation = computed(() => {
    return this.trucks().filter((t) => t.currentLocation?.latitude && t.currentLocation?.longitude)
      .length;
  });

  protected readonly trucksWithoutLocation = computed(() => {
    return this.trucks().filter(
      (t) => !t.currentLocation?.latitude || !t.currentLocation?.longitude,
    ).length;
  });
}
