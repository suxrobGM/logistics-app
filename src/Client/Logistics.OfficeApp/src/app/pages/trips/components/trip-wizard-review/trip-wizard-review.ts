import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TripStopType } from "@/core/api/models";
import { DirectionMap } from "@/shared/components";
import type {
  RouteSegmentClickEvent,
  WaypointClickEvent,
} from "@/shared/components/direction-map/types";
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

  protected stopLabel(tripStopType: TripStopType): string {
    return tripStopType === TripStopType.PickUp ? "Pick Up" : "Drop Off";
  }

  protected onRouteSegmentClick(e: RouteSegmentClickEvent): void {
    const stop = this.reviewData().stops.find((s: { id: string }) => s.id === e.fromWaypoint.id);
    this.store.setSelectedStop(stop ?? null);
  }

  protected onWaypointClick(e: WaypointClickEvent): void {
    const stop = this.reviewData().stops.find((s: { id: string }) => s.id === e.waypoint.id);
    this.store.setSelectedStop(stop ?? null);
  }

  protected goToPreviousStep(): void {
    this.store.previousStep();
  }

  protected reOptimizeRoute(): void {
    this.store.reOptimizeStops();
  }
}
