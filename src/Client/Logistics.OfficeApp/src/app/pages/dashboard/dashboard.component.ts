import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {environment} from "@/env";
import {GrossesBarchart, TrucksMap} from "@/shared/components";
import {CompanyStatsComponent, TruckStatsTableComponent} from "./components";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    TruckStatsTableComponent,
    GrossesBarchart,
    CompanyStatsComponent,
    TrucksMap,
  ],
})
export class DashboardComponent {
  public readonly accessToken: string;

  constructor() {
    this.accessToken = environment.mapboxToken;
  }
}
