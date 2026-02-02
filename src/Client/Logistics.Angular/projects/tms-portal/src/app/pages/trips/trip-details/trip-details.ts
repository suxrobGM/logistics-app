import { CommonModule } from "@angular/common";
import { Component, computed, effect, inject, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { TripStopDto, TripStopType } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressBarModule } from "primeng/progressbar";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { ToastModule } from "primeng/toast";
import {
  DirectionMap,
  LoadStatusTag,
  type RouteSegmentClickEvent,
  TripStatusTag,
  type Waypoint,
  type WaypointClickEvent,
} from "@/shared/components";
import { TripActions, TripTimeline } from "../components";
import { TripDetailsStore } from "../store/trip-details.store";

@Component({
  selector: "app-trip-details",
  templateUrl: "./trip-details.html",
  providers: [TripDetailsStore],
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    TagModule,
    TableModule,
    ButtonModule,
    DirectionMap,
    DistanceUnitPipe,
    DateFormatPipe,
    CurrencyFormatPipe,
    LoadStatusTag,
    AddressPipe,
    SkeletonModule,
    TripStatusTag,
    ProgressBarModule,
    ToastModule,
    TripTimeline,
    TripActions,
  ],
})
export class TripDetailsPage {
  protected readonly store = inject(TripDetailsStore);
  private readonly toastService = inject(ToastService);
  private readonly localizationService = inject(LocalizationService, { optional: true });

  protected readonly tripId = input<string>();

  protected readonly distanceUnitLabel = computed(
    () => this.localizationService?.getDistanceUnitLabel() ?? "mi",
  );

  // Transform TripStopDto[] to Waypoint[] for DirectionMap
  protected readonly waypoints = computed<Waypoint[]>(() =>
    this.store
      .sortedStops()
      .filter((stop) => stop.id != null)
      .map((stop) => ({
        id: stop.id!,
        location: stop.location,
      })),
  );

  // Transform selectedStop to Waypoint | null for DirectionMap
  protected readonly selectedWaypoint = computed<Waypoint | null>(() => {
    const stop = this.store.selectedStop();
    if (!stop || !stop.id) return null;
    return { id: stop.id, location: stop.location };
  });

  constructor() {
    // React to tripId changes
    effect(() => {
      const id = this.tripId();
      if (id) {
        this.store.loadTrip(id);
      }
    });
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent): void {
    const stop = this.store.trip()?.stops?.find((s) => s.id === e.fromWaypoint.id);
    this.store.selectStop(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent): void {
    const stop = this.store.trip()?.stops?.find((s) => s.id === e.waypoint.id);
    this.store.selectStop(stop ?? null);
  }

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === "pick_up" ? "Pick Up" : "Drop Off";
  }

  protected async onDispatch(): Promise<void> {
    const success = await this.store.dispatchTrip();
    if (success) {
      this.toastService.showSuccess(
        "The trip has been dispatched successfully.",
        "Trip Dispatched",
      );
    } else {
      this.toastService.showError(this.store.error() ?? "Failed to dispatch trip.");
    }
  }

  protected async onCancel(reason?: string): Promise<void> {
    const success = await this.store.cancelTrip(reason);
    if (success) {
      this.toastService.showInfo("The trip has been cancelled.", "Trip Cancelled");
    } else {
      this.toastService.showError(this.store.error() ?? "Failed to cancel trip.");
    }
  }

  protected async onMarkStopArrived(stop: TripStopDto): Promise<void> {
    if (!stop.id) return;

    const success = await this.store.markStopArrived(stop.id);
    if (success) {
      this.toastService.showSuccess("Stop marked as arrived.", "Stop Marked");
    } else {
      this.toastService.showError(this.store.error() ?? "Failed to mark stop as arrived.");
    }
  }
}
