import {CommonModule} from "@angular/common";
import {Component, OnInit, computed, inject, input, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {SkeletonModule} from "primeng/skeleton";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {TripDto, TripStopDto, TripStopType} from "@/core/api/models";
import {DirectionMap, LoadStatusTag, LoadTypeTag, TripStatusTag} from "@/shared/components";
import {RouteSegmentClickEvent, WaypointClickEvent} from "@/shared/components/direction-map/types";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

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
  protected readonly selectedStop = signal<TripStopDto | null>(null);

  protected readonly sortedStops = computed<TripStopDto[]>(() => {
    return (this.trip()?.stops ?? []).slice().sort((a, b) => a.order - b.order);
  });

  ngOnInit(): void {
    this.fetchTrip();
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent) {
    const stop = this.trip()?.stops.find((s) => s.id === e.fromWaypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent) {
    const stop = this.trip()?.stops.find((s) => s.id === e.waypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === TripStopType.PickUp ? "Pick Up" : "Drop Off";
  }

  private fetchTrip(): void {
    const id = this.tripId();

    if (!id) {
      return;
    }

    this.isLoading.set(true);
    this.apiService.tripApi.getTrip(id).subscribe((result) => {
      if (result.success && result.data) {
        this.trip.set(result.data);
      }

      this.isLoading.set(false);
    });
  }
}
