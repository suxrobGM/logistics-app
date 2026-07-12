import { Component, computed, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import { Card } from "../card/card";

type ColorVariant = "blue" | "green" | "orange" | "red" | "purple" | "gray";

const COLOR_CLASSES: Record<ColorVariant, string> = {
  blue: "text-blue-600 dark:text-blue-400",
  green: "text-green-600 dark:text-green-400",
  orange: "text-orange-600 dark:text-orange-400",
  red: "text-red-600 dark:text-red-400",
  purple: "text-purple-600 dark:text-purple-400",
  gray: "text-gray-600 dark:text-gray-400",
};

@Component({
  selector: "ui-dashboard-card",
  templateUrl: "./dashboard-card.html",
  imports: [Card, Icon],
})
export class DashboardCard {
  public readonly title = input.required<string>();
  public readonly icon = input<IconName | null>(null);
  public readonly iconColor = input<ColorVariant>("blue");

  /** Colour + spacing only. The glyph itself is `<ui-icon [name]>`, never a class string. */
  protected readonly iconClasses = computed(() => `mr-2 ${COLOR_CLASSES[this.iconColor()]}`);
}
