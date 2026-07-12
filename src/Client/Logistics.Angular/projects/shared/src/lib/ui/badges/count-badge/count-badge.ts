import { Component, computed, input } from "@angular/core";
import { classes } from "../../primitives/utils";
import type { UiBadgeIntent } from "../badge/badge-intent";
import { TONE_CLASSES } from "../badge/badge-variants";

export type UiCountBadgeSize = "small" | "normal";

/**
 * The count pill, and the badge behind `<ui-overlay-badge>`.
 *
 * Deliberately NOT `ui-badge`, which grows with its text: the fixed height and min-width here are the
 * point, since they make a single-digit count render as a circle and a double-digit one as a pill.
 * It does share `ui-badge`'s `TONE_CLASSES` — same seven colours, different shape.
 */
@Component({
  selector: "ui-count-badge",
  templateUrl: "./count-badge.html",
})
export class CountBadge {
  public readonly value = input<string | number | null>(null);
  public readonly severity = input<UiBadgeIntent>("danger");
  public readonly size = input<UiCountBadgeSize>("normal");

  protected readonly text = computed(() => {
    const value = this.value();
    return value === null || value === "" ? null : String(value);
  });

  constructor() {
    classes(() => [
      "inline-flex shrink-0 items-center justify-center rounded-md font-bold leading-none",
      this.size() === "small" ? "h-5 min-w-5 px-1.5 text-[0.625rem]" : "h-6 min-w-6 px-2 text-xs",
      TONE_CLASSES[this.severity()],
    ]);
  }
}
