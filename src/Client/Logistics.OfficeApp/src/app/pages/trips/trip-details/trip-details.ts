import {CommonModule} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {SkeletonModule} from "primeng/skeleton";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {TripDto, TripStopType} from "@/core/api/models";
import {DirectionMap, LoadStatusTag, LoadTypeTag, TripStatusTag} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";
import {GeoPoint} from "@/shared/types/mapbox";

@Component({
  selector: "app-trip-details",
  templateUrl: "./trip-details.html",
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    TagModule,
    TableModule,
    ButtonModule,
    DirectionMap,
    DistanceUnitPipe,
    LoadStatusTag,
    AddressPipe,
    SkeletonModule,
    TripStatusTag,
    LoadTypeTag,
  ],
})
export class TripDetailsPage implements OnInit {
  private readonly apiService = inject(ApiService);

  protected readonly tripId = input<string>();

  protected readonly isLoading = signal<boolean>(false);
  protected readonly trip = signal<TripDto | null>(null);

  ngOnInit() {
    this.fetchTrip();
  }

  /**
   * Returns the coordinates of the trip stops in the order they are defined.
   * @param stops Trip stops to extract coordinates from.
   * @returns Array of coordinates in the format [longitude, latitude].
   */
  protected tripStopCoords(): GeoPoint[] {
    return (
      this.trip()
        ?.stops.sort((a, b) => a.order - b.order)
        .map((s) => [s.location.longitude, s.location.latitude] as GeoPoint) ?? []
    );
  }

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === TripStopType.PickUp ? "Pick Up" : "Drop Off";
  }

  private fetchTrip(): void {
    const id = this.tripId();
    console.log("Fetching trip with ID:", id);

    if (!id) {
      return;
    }

    this.isLoading.set(true);
    this.apiService.tripApi.getTrip(id).subscribe((result) => {
      if (result.success && result.data) {
        console.log("Trip data:", result.data);

        this.trip.set(result.data);
      }

      this.isLoading.set(false);
    });
  }
}
