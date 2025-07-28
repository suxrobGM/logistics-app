import {Component, signal} from "@angular/core";
import {CardModule} from "primeng/card";
import {TripForm} from "../components";

@Component({
  selector: "app-trip-add",
  templateUrl: "./trip-add.html",
  imports: [CardModule, TripForm],
})
export class TripAddPage {
  protected readonly isLoading = signal(false);
}
