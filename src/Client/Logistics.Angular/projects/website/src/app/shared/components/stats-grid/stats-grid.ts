import { Component, input } from "@angular/core";
import { AnimatedCounter } from "../animated-counter/animated-counter";

export interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-stats-grid",
  templateUrl: "./stats-grid.html",
  imports: [AnimatedCounter],
})
export class StatsGrid {
  public readonly stats = input.required<StatItem[]>();
}
