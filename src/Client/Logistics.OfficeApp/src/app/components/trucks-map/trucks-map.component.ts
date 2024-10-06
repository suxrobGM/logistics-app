/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {Component, Input, OnDestroy, OnInit} from "@angular/core";
import {CommonModule} from "@angular/common";
import {GeolocationMapComponent} from "@/components";
import {TruckGeolocationDto} from "@/core/models";
import {ApiService, LiveTrackingService} from "@/core/services";

@Component({
  selector: "app-trucks-map",
  standalone: true,
  templateUrl: "./trucks-map.component.html",
  styleUrls: [],
  imports: [CommonModule, GeolocationMapComponent],
})
export class TrucksMapComponent implements OnInit, OnDestroy {
  public truckLocations: TruckGeolocationDto[];

  @Input({required: true}) accessToken!: string;
  @Input() width: string;
  @Input() height: string;

  constructor(
    private apiService: ApiService,
    private liveTrackingService: LiveTrackingService
  ) {
    this.truckLocations = [];
    this.width = "100%";
    this.height = "100%";
  }

  ngOnInit(): void {
    this.fetchTrucksData();
    this.connectToLiveTracking();
  }

  ngOnDestroy(): void {
    this.liveTrackingService.disconnect();
  }

  private connectToLiveTracking() {
    this.liveTrackingService.connect();

    this.liveTrackingService.onReceiveGeolocationData = (data: TruckGeolocationDto) => {
      const index = this.truckLocations.findIndex((loc) => loc.truckId === data.truckId);

      if (index !== -1) {
        this.truckLocations[index] = data;
      } else {
        this.truckLocations.push(data);
      }
    };
  }

  private fetchTrucksData() {
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

      this.truckLocations = truckLocations;
    });
  }
}
