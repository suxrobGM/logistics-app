import {Component, input} from "@angular/core";
import {CardModule} from "primeng/card";

@Component({
  selector: "app-trip-edit",
  templateUrl: "./trip-edit.html",
  imports: [CardModule],
})
export class TripEditPage {
  protected readonly tripId = input<string>();
}
