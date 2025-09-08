import {CurrencyPipe, DatePipe} from "@angular/common";
import {Component, input, output, signal} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {TableModule} from "primeng/table";
import {TripStopDto, TripStopType} from "@/core/api/models";
import {DirectionMap} from "@/shared/components";
import type {
  RouteSegmentClickEvent,
  WaypointClickEvent,
} from "@/shared/components/direction-map/types";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

export interface TripWizardReviewData {
  tripName: string;
  truckId: string;
  totalLoads: number;
  newLoadsCount: number;
  pendingDetachLoadsCount: number;
  totalDistance: number;
  totalCost: number;
  stops: TripStopDto[];
}

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
  public readonly stepData = input<TripWizardReviewData | null>(null);
  public readonly saveButtonLabel = input<string>("Create");

  public readonly back = output<void>();
  public readonly save = output<void>();

  protected readonly selectedStop = signal<TripStopDto | null>(null);

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === TripStopType.PickUp ? "Pick Up" : "Drop Off";
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent) {
    const stop = this.stepData()?.stops.find((s) => s.id === e.fromWaypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent) {
    const stop = this.stepData()?.stops.find((s) => s.id === e.waypoint.id);
    this.selectedStop.set(stop ?? null);
  }
}
