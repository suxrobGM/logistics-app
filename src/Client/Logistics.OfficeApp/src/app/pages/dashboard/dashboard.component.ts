import {Component} from "@angular/core";
import {CommonModule} from "@angular/common";
import {GLOBAL_CONFIG} from "@/configs";
import {GrossesBarchartComponent, TrucksMapComponent} from "@/components";
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
    this.accessToken = GLOBAL_CONFIG.mapboxToken;
  }
}
