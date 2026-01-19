import { Component, computed, input } from "@angular/core";
import { CardModule } from "primeng/card";

type ColorVariant = "blue" | "green" | "orange" | "red" | "purple" | "gray";

@Component({
  selector: "ui-dashboard-card",
  templateUrl: "./dashboard-card.html",
  imports: [CardModule],
})
export class DashboardCard {
  public readonly title = input.required<string>();
  public readonly icon = input<string | null>(null);
  public readonly iconColor = input<ColorVariant>("blue");

  protected readonly iconClasses = computed(() => {
    if (!this.icon()) return "";

    const colorMap: Record<ColorVariant, string> = {
      blue: "text-blue-600",
      green: "text-green-600",
      orange: "text-orange-600",
      red: "text-red-600",
      purple: "text-purple-600",
      gray: "text-gray-600",
    };
    return `pi ${this.icon()} mr-2 ${colorMap[this.iconColor()]}`;
  });
}
