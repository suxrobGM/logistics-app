import { DatePipe } from "@angular/common";
import { Component, computed, inject, input, output } from "@angular/core";
import type { TripStopDto, TripStopType } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  Grid,
  Icon,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiSortHeader,
  UiTableRowDirectives,
} from "@logistics/shared/ui";
import {
  DirectionMap,
  type RouteSegmentClickEvent,
  type Waypoint,
  type WaypointClickEvent,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { TripWizardStore } from "../../store/trip-wizard-store";

@Component({
  selector: "app-trip-wizard-review",
  templateUrl: "./trip-wizard-review.html",
  imports: [
    AddressPipe,
    Card,
    CurrencyFormatPipe,
    DatePipe,
    DirectionMap,
    DistanceUnitPipe,
    Grid,
    Icon,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiSortHeader,
    UiTableRowDirectives,
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
