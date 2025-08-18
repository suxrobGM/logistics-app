import {CurrencyPipe, DatePipe} from "@angular/common";
import {Component, input, output} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {Message, MessageModule} from "primeng/message";
import {DirectionMap} from "@/shared/components";
import {DistanceUnitPipe} from "@/shared/pipes";

@Component({
  selector: "app-trip-form-step-review",
  templateUrl: "./trip-form-step-review.html",
  imports: [
    CardModule,
    DirectionMap,
    ButtonModule,
    DistanceUnitPipe,
    CurrencyPipe,
    DatePipe,
    MessageModule,
  ],
})
export class TripFormStepReview {
  public readonly tripName = input<string>();
  public readonly plannedStart = input<Date>();
  public readonly truckId = input<string>();

  public readonly totalCount = input<number>(0);
  public readonly newCount = input<number>(0);
  public readonly pendingDetachCount = input<number>(0);
  public readonly totalDistance = input<number>(0);
  public readonly totalCost = input<number>(0);
  public readonly stopCoords = input<[number, number][]>([]);
  public readonly mode = input<"create" | "edit">("create");
  public readonly warnings = input<Message[]>([]);

  public readonly back = output<void>();
  public readonly save = output<void>();
}
