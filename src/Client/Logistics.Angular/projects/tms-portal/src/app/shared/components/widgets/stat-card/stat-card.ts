import { Component, computed, input } from "@angular/core";
import { Card, Icon, Stack, Typography, UiTooltip, type IconName } from "@logistics/shared/ui";

type ColorVariant = "blue" | "green" | "orange" | "red" | "purple" | "gray";

const COLOR_VAR_MAP: Record<ColorVariant, string> = {
  blue: "var(--info)",
  green: "var(--success)",
  orange: "var(--warning)",
  red: "var(--danger)",
  purple: "var(--status-pickedup)",
  gray: "var(--text-muted)",
};

@Component({
  selector: "app-stat-card",
  templateUrl: "./stat-card.html",
  styleUrl: "./stat-card.css",
  imports: [Card, Icon, Stack, Typography, UiTooltip],
  host: {
    "[style.--stat-card-icon-color]": "iconColorVar()",
  },
})
export class StatCard {
  public readonly icon = input.required<IconName>();
  public readonly label = input.required<string>();
  public readonly value = input.required<string | number>();
  public readonly color = input<ColorVariant>("blue");
  public readonly trend = input<string | null>(null);
  public readonly trendDirection = input<"up" | "down" | null>(null);
  public readonly tooltip = input<string | null>(null);

  protected readonly iconColorVar = computed(() => COLOR_VAR_MAP[this.color()]);

  protected readonly trendClasses = computed(() => {
    if (this.trendDirection() === "up") return "text-success";
    if (this.trendDirection() === "down") return "text-danger";
    return "text-muted-foreground";
  });

  protected readonly trendIcon = computed<IconName | null>(() => {
    if (this.trendDirection() === "up") return "arrow-up";
    if (this.trendDirection() === "down") return "arrow-down";
    return null;
  });
}
