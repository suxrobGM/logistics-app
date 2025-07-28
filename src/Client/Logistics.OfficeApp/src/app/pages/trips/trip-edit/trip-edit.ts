import {Component, input, signal} from "@angular/core";
import {CardModule} from "primeng/card";
import {TripForm} from "../components";

@Component({
  selector: "app-trip-edit",
  templateUrl: "./trip-edit.html",
  imports: [CardModule, TripForm],
})
export class TripEditPage {
  protected readonly tripId = input<string>();

  protected readonly isLoading = signal(false);
}
