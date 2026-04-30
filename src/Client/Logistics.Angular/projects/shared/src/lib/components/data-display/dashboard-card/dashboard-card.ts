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
      blue: "text-blue-600 dark:text-blue-400",
      green: "text-green-600 dark:text-green-400",
      orange: "text-orange-600 dark:text-orange-400",
      red: "text-red-600 dark:text-red-400",
      purple: "text-purple-600 dark:text-purple-400",
      gray: "text-gray-600 dark:text-gray-400",
    };
    return `pi ${this.icon()} mr-2 ${colorMap[this.iconColor()]}`;
  });
}
