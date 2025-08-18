import {CurrencyPipe} from "@angular/common";
import {Component, input, output} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DirectionMap} from "@/shared/components";
import {DistanceUnitPipe} from "@/shared/pipes";
import {GeoPoint} from "@/shared/types/mapbox";

@Component({
  selector: "app-trip-form-step-review",
  templateUrl: "./trip-form-step-review.html",
  imports: [CardModule, DirectionMap, ButtonModule, DistanceUnitPipe, CurrencyPipe],
})
export class TripFormStepReview {
  public readonly tripName = input<string>();
  public readonly truckId = input<string>();

  public readonly totalLoads = input<number>(0);
  public readonly newLoadsCount = input<number>(0);
  public readonly pendingDetachLoadsCount = input<number>(0);
  public readonly totalDistance = input<number>(0);
  public readonly totalCost = input<number>(0);
  public readonly stopCoords = input<GeoPoint[]>([]);
  public readonly mode = input<"create" | "edit">("create");

  public readonly back = output<void>();
  public readonly save = output<void>();
}
