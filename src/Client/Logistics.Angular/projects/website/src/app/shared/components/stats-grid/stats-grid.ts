import { Component, input } from "@angular/core";

export interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-stats-grid",
  templateUrl: "./stats-grid.html",
})
export class StatsGrid {
  public readonly stats = input.required<StatItem[]>();
}
