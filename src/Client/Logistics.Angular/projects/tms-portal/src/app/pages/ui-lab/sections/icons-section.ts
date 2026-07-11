import { Component } from "@angular/core";
import {
  BASE_ICON_NAMES,
  Icon,
  ICON_ALIASES,
  TMS_ICON_NAMES,
  toNgIconName,
  Typography,
  type IconColor,
  type IconSize,
  type UiIconName,
} from "@logistics/shared/ui";

/** One cell of the grid: the name we hand `<ui-icon>`, plus the glyph it must resolve to. */
export interface LabIcon {
  /** The `name` passed to `<ui-icon>`. */
  readonly name: UiIconName;
  /** The `@ng-icons` export it must resolve to. Shown as the cell's tooltip. */
  readonly exportName: string;
}

/**
 * One cell per DISTINCT glyph — several names resolve to the same one (`exclamation-circle` and
 * `alert-circle` both land on `lucideCircleAlert`), and the grid tracks by `exportName`. `seen` carries
 * across calls so the TMS grid shows only what TMS adds on top of base.
 */
function glyphsOf(names: readonly UiIconName[], seen: Set<string>): LabIcon[] {
  const cells: LabIcon[] = [];

  for (const name of [...names].sort()) {
    const exportName = toNgIconName(ICON_ALIASES[name]);
    if (seen.has(exportName)) continue;
    seen.add(exportName);
    cells.push({ name, exportName });
  }

  return cells;
}

/**
 * Every icon this portal has registered, rendered through `<ui-icon>` and driven off the GENERATED
 * registry (`tools/gen-icon-registry.mjs`) — never a hand-typed list, so an icon added to
 * `icon-map.json` shows up here for free.
 *
 * Each cell has a thin border: an icon that fails to resolve renders the error glyph (or nothing), and a
 * bordered box makes either impossible to miss — a blank in an unbordered grid is invisible. That is the
 * whole point of the section: the S2/S3 icon sweep is exactly the kind of change that stays green on
 * `build` and on the test suite while quietly blanking a glyph.
 */
@Component({
  selector: "app-ui-lab-icons",
  templateUrl: "./icons-section.html",
  imports: [Icon, Typography],
})
export class UiLabIconsSection {
  private readonly seen = new Set<string>();

  /** Glyphs every portal registers via `BASE_NG_ICONS`. */
  protected readonly baseIcons = glyphsOf(BASE_ICON_NAMES, this.seen);

  /** TMS-only additions (`TMS_NG_ICONS`) — the ones a shared-lib-only registry would miss. */
  protected readonly tmsIcons = glyphsOf(TMS_ICON_NAMES, this.seen);

  /**
   * The names that are not their own glyph: the legacy PrimeIcons-style aliases call sites still pass
   * (`cog` -> `settings`, `times` -> `x`, `exclamation-triangle` -> `triangle-alert`). ~200 call sites
   * write these today, so a blank here is a blank on a real page.
   */
  protected readonly legacyAliases: LabIcon[] = [
    ...new Set<UiIconName>([...BASE_ICON_NAMES, ...TMS_ICON_NAMES]),
  ]
    .filter((name) => ICON_ALIASES[name] !== name)
    .sort()
    .map((name) => ({ name, exportName: toNgIconName(ICON_ALIASES[name]) }));

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

  protected readonly totalRegistered = this.baseIcons.length + this.tmsIcons.length;
}
