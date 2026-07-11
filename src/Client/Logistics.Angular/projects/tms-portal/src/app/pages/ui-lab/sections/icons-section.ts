import { Component } from "@angular/core";
import {
  BASE_NG_ICONS,
  Icon,
  PI_TO_LUCIDE,
  Typography,
  type IconColor,
  type IconSize,
} from "@logistics/shared/ui";
import { TMS_NG_ICONS } from "@/shared/icons/lucide-icons";

/** One cell of the grid: the name we hand `<ui-icon>`, plus where it came from. */
export interface LabIcon {
  /** The `name` passed to `<ui-icon>`. */
  readonly name: string;
  /** The `@ng-icons/lucide` export it must resolve to. Shown as the cell's tooltip. */
  readonly exportName: string;
}

/**
 * `lucideChevronDown` → `chevron-down`, `lucideBuilding2` → `building-2`.
 * The exact inverse of the shared `toNgIconName()`, so every name we render round-trips back to
 * the registry key it came from.
 */
function toKebab(exportName: string): string {
  return exportName
    .replace(/^lucide/, "")
    .replace(/([A-Za-z])(\d)/g, "$1-$2")
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .toLowerCase();
}

function iconsFrom(
  registry: Record<string, unknown>,
  exclude: Record<string, unknown> = {},
): LabIcon[] {
  return Object.keys(registry)
    .filter((key) => !(key in exclude))
    .sort()
    .map((exportName) => ({ name: toKebab(exportName), exportName }));
}

/**
 * Every icon this portal has registered, rendered through `<ui-icon>` and driven off the registries
 * themselves — never a hand-typed list, so an icon added to `BASE_NG_ICONS` or `TMS_NG_ICONS`
 * shows up here for free.
 *
 * Each cell has a thin border: an icon that fails to resolve renders *nothing*, and an empty
 * bordered box is impossible to miss (a blank in an unbordered grid is invisible). That is the
 * whole point of the section — the S2/S3 icon sweep is exactly the kind of change that stays green
 * on `build` and on the test suite while quietly blanking a glyph.
 */
@Component({
  selector: "app-ui-lab-icons",
  templateUrl: "./icons-section.html",
  imports: [Icon, Typography],
})
export class UiLabIconsSection {
  /** Icons every portal registers via `BASE_NG_ICONS`. */
  protected readonly baseIcons = iconsFrom(BASE_NG_ICONS);

  /** TMS-only additions — the ones a shared-lib-only registry would miss. */
  protected readonly tmsIcons = iconsFrom(TMS_NG_ICONS, BASE_NG_ICONS);

  /**
   * The legacy PrimeIcons-style aliases `<ui-icon>` still accepts (`cog`, `times`, `bolt`, …).
   * ~200 call sites pass these today, so a blank here is a blank on a real page.
   */
  protected readonly legacyAliases: LabIcon[] = Object.keys(PI_TO_LUCIDE)
    .sort()
    .map((alias) => ({ name: alias, exportName: PI_TO_LUCIDE[alias] }));

  protected readonly sizes: readonly IconSize[] = ["xs", "sm", "md", "lg", "xl"];

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

  protected readonly totalRegistered = this.baseIcons.length + this.tmsIcons.length;
}
