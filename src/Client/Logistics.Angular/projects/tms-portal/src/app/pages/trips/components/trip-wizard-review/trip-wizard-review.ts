import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, input, output } from "@angular/core";
import type { TripStopDto, TripStopType } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import {
  DirectionMap,
  type RouteSegmentClickEvent,
  type Waypoint,
  type WaypointClickEvent,
} from "@/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@/shared/pipes";
import { TripWizardStore } from "../../store/trip-wizard-store";

@Component({
  selector: "app-trip-wizard-review",
  templateUrl: "./trip-wizard-review.html",
  imports: [
    CardModule,
    DirectionMap,
    ButtonModule,
    DistanceUnitPipe,
    CurrencyPipe,
    TableModule,
    AddressPipe,
    DatePipe,
  ],
})
export class TripWizardReview {
  protected readonly store = inject(TripWizardStore);

  public readonly saveButtonLabel = input<string>("Create");
  public readonly save = output<void>();

  // Expose store state for template
  protected readonly reviewData = this.store.reviewData;
  protected readonly isOptimizing = this.store.isOptimizing;
  protected readonly selectedStop = this.store.selectedStop;

  // Transform TripStopDto[] to Waypoint[] for DirectionMap
  protected readonly waypoints = computed<Waypoint[]>(() =>
    this.reviewData()
      .stops.filter((stop) => stop.id != null)
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

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === "pick_up" ? "Pick Up" : "Drop Off";
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent): void {
    const stop = this.reviewData().stops.find((s: TripStopDto) => s.id === e.fromWaypoint.id);
    this.store.setSelectedStop(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent): void {
    const stop = this.reviewData().stops.find((s: TripStopDto) => s.id === e.waypoint.id);
    this.store.setSelectedStop(stop ?? null);
  }

  protected goToPreviousStep(): void {
    this.store.previousStep();
  }

  protected reOptimizeRoute(): void {
    this.store.reOptimizeStops();
  }
}
