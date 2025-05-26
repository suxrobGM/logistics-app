import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {GrossesBarchartComponent, TrucksMapComponent} from "@/components";
import {environment} from "@/env";
import {CompanyStatsComponent, TruckStatsTableComponent} from "./components";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    TruckStatsTableComponent,
    GrossesBarchartComponent,
    CompanyStatsComponent,
    TrucksMapComponent,
  ],
})
export class DashboardComponent {
  public readonly accessToken: string;

  constructor() {
    this.accessToken = environment.mapboxToken;
  }
}
