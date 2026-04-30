import { Component, computed, input } from "@angular/core";
import { CardModule } from "primeng/card";

type ColorVariant = "blue" | "green" | "orange" | "red" | "purple" | "gray";

@Component({
  selector: "ui-stat-card",
  templateUrl: "./stat-card.html",
  imports: [CardModule],
})
export class StatCard {
  public readonly icon = input.required<string>();
  public readonly label = input.required<string>();
  public readonly value = input.required<string | number>();
  public readonly color = input<ColorVariant>("blue");
  public readonly trend = input<string | null>(null);
  public readonly trendDirection = input<"up" | "down" | null>(null);

  protected readonly iconClasses = computed(() => {
    const colorMap: Record<ColorVariant, string> = {
      blue: "text-blue-600 dark:text-blue-400",
      green: "text-green-600 dark:text-green-400",
      orange: "text-orange-600 dark:text-orange-400",
      red: "text-red-600 dark:text-red-400",
      purple: "text-purple-600 dark:text-purple-400",
      gray: "text-gray-600 dark:text-gray-400",
    };
    return `pi ${this.icon()} text-2xl ${colorMap[this.color()]}`;
  });

  protected readonly bgClasses = computed(() => {
    const colorMap: Record<ColorVariant, string> = {
      blue: "bg-blue-600/10 dark:bg-blue-400/15",
      green: "bg-green-600/10 dark:bg-green-400/15",
      orange: "bg-orange-600/10 dark:bg-orange-400/15",
      red: "bg-red-600/10 dark:bg-red-400/15",
      purple: "bg-purple-600/10 dark:bg-purple-400/15",
      gray: "bg-gray-600/10 dark:bg-gray-400/15",
    };
    return `flex h-14 w-14 shrink-0 items-center justify-center rounded-full ${colorMap[this.color()]}`;
  });

  protected readonly trendClasses = computed(() => {
    if (this.trendDirection() === "up") {
      return "text-green-600 dark:text-green-400";
    }
    if (this.trendDirection() === "down") {
      return "text-red-600 dark:text-red-400";
    }
    return "text-gray-500 dark:text-gray-400";
  });

  protected readonly trendIcon = computed(() => {
    if (this.trendDirection() === "up") {
      return "pi pi-arrow-up";
    }
    if (this.trendDirection() === "down") {
      return "pi pi-arrow-down";
    }
    return "";
  });
}
