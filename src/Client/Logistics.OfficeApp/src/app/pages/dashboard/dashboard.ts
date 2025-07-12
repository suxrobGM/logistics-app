import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {GrossesBarchart, TrucksMap} from "@/shared/components";
import {CompanyStatsComponent, TruckStatsTableComponent} from "./components";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.html",
  imports: [
    CommonModule,
    TruckStatsTableComponent,
    GrossesBarchart,
    CompanyStatsComponent,
    TrucksMap,
  ],
})
export class DashboardComponent {}
