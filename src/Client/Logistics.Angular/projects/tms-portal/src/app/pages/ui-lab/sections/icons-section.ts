import { Component } from "@angular/core";
import {
  Icon,
  Typography,
  UI_ICONS,
  type IconColor,
  type IconName,
  type IconSize,
} from "@logistics/shared/ui";

/**
 * Every icon the app can render, straight off `UI_ICONS` — never a hand-typed list, so an icon added
 * to `icons.ts` shows up here for free.
 *
 * Each cell is bordered on purpose: an icon that fails to resolve renders the error glyph (or nothing),
 * and a bordered box makes either impossible to miss — a blank in an unbordered grid is invisible.
 */
@Component({
  selector: "app-ui-lab-icons",
  templateUrl: "./icons-section.html",
  imports: [Icon, Typography],
})
export class UiLabIconsSection {
  protected readonly icons = (Object.keys(UI_ICONS) as IconName[]).sort();

  protected readonly sizes: readonly IconSize[] = ["inherit", "xs", "sm", "md", "lg", "xl"];

  protected readonly colors: readonly IconColor[] = [
    "inherit",
    "primary",
    "secondary",
    "muted",
    "success",
    "warning",
    "danger",
    "info",
  ];
}
