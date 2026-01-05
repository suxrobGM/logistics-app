import { CommonModule } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { Api, getTripById$Json } from "@/core/api";
import type { TripDto, TripStopDto, TripStopType } from "@/core/api/models";
import { DirectionMap, LoadStatusTag, LoadTypeTag, TripStatusTag } from "@/shared/components";
import type {
  RouteSegmentClickEvent,
  Waypoint,
  WaypointClickEvent,
} from "@/shared/components/direction-map/types";
import { AddressPipe, DistanceUnitPipe } from "@/shared/pipes";

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
  private readonly api = inject(Api);

  protected readonly tripId = input<string>();

  protected readonly isLoading = signal<boolean>(false);
  protected readonly trip = signal<TripDto | null>(null);
  protected readonly selectedStop = signal<TripStopDto | null>(null);

  protected readonly sortedStops = computed<TripStopDto[]>(() => {
    return (this.trip()?.stops ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
  });

  // Transform TripStopDto[] to Waypoint[] for DirectionMap
  protected readonly waypoints = computed<Waypoint[]>(() =>
    this.sortedStops()
      .filter((stop) => stop.id != null)
      .map((stop) => ({
        id: stop.id!,
        location: stop.location,
      })),
  );

  // Transform selectedStop to Waypoint | null for DirectionMap
  protected readonly selectedWaypoint = computed<Waypoint | null>(() => {
    const stop = this.selectedStop();
    if (!stop || !stop.id) return null;
    return { id: stop.id, location: stop.location };
  });

  ngOnInit(): void {
    this.fetchTrip();
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent) {
    const stop = this.trip()?.stops?.find((s) => s.id === e.fromWaypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent) {
    const stop = this.trip()?.stops?.find((s) => s.id === e.waypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === "pick_up" ? "Pick Up" : "Drop Off";
  }

  private async fetchTrip(): Promise<void> {
    const id = this.tripId();

    if (!id) {
      return;
    }

    this.isLoading.set(true);
    const result = await this.api.invoke(getTripById$Json, { tripId: id });
    if (result.success && result.data) {
      this.trip.set(result.data);
    }

    this.isLoading.set(false);
  }
}
