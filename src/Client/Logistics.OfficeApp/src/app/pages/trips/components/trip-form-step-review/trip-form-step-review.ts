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

@Component({
  selector: "app-trip-form-step-review",
  templateUrl: "./trip-form-step-review.html",
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
export class TripFormStepReview {
  public readonly tripName = input<string>();
  public readonly truckId = input<string>();
  public readonly totalLoads = input<number>(0);
  public readonly newLoadsCount = input<number>(0);
  public readonly pendingDetachLoadsCount = input<number>(0);
  public readonly totalDistance = input<number>(0);
  public readonly totalCost = input<number>(0);
  public readonly stops = input<TripStopDto[]>([]);
  public readonly mode = input<"create" | "edit">("create");

  public readonly back = output<void>();
  public readonly save = output<void>();

  protected readonly selectedStop = signal<TripStopDto | null>(null);

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === TripStopType.PickUp ? "Pick Up" : "Drop Off";
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent) {
    const stop = this.stops().find((s) => s.id === e.fromWaypoint.id);
    this.selectedStop.set(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent) {
    const stop = this.stops().find((s) => s.id === e.waypoint.id);
    this.selectedStop.set(stop ?? null);
  }
}
