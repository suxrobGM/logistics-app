import { booleanAttribute, Component, computed, input } from "@angular/core";
import { classes } from "../../primitives/utils";

export type DividerOrientation = "horizontal" | "vertical";

/**
 * Horizontal or vertical separator with optional inline label via `<ng-content>`.
 *
 * Absorbs `<p-divider>` (58 sites), which comes in exactly two shapes: `<p-divider />` (43) and
 * `<p-divider class="m-0" />` (15). Nothing else — no `layout`, no `type`, no `align`.
 *
 * =================================================================================================
 * TWO THINGS HAD TO CHANGE FOR THOSE 15 `class="m-0"` SITES, AND BOTH WOULD HAVE FAILED SILENTLY.
 * =================================================================================================
 * 1. THE MARGIN MOVED TO THE HOST. It lived on an inner `<div>`, so `class="m-0"` on `<ui-divider>`
 *    would land on the host and cancel nothing — the inner `my-3` survives and the divider keeps its
 *    margin. p-divider put the margin on its own ROOT
 *    (`.p-divider-horizontal { margin: dt('divider.horizontal.margin') }`), which is exactly why
 *    `m-0` worked there. It is on the host here now, applied through `classes()` so a call site's
 *    `m-0` twMerges and WINS instead of racing our class in stylesheet order.
 *
 * 2. THE DEFAULT MARGIN IS `my-4` (1rem), NOT `my-3`. That is p-divider's
 *    `divider.horizontal.margin: 1rem 0`. Keeping `my-3` would have shifted all 58 incoming dividers
 *    by 4px in order to spare the 4 `<ui-divider>`s that already exist — two of which are in /ui-lab.
 *    The bigger population wins; the two real existing ones move by 4px.
 * =================================================================================================
 */
@Component({
  selector: "ui-divider",
  templateUrl: "./divider.html",
  host: { role: "separator" },
})
export class Divider {
  public readonly orientation = input<DividerOrientation>("horizontal");
  public readonly inset = input<boolean, unknown>(false, { transform: booleanAttribute });

  protected readonly isHorizontal = computed(() => this.orientation() === "horizontal");

  protected readonly lineClasses = computed(() =>
    this.isHorizontal() ? "flex-1 border-t border-default" : "flex-1 border-l border-default",
  );

  constructor() {
    classes(() => [
      this.isHorizontal()
        ? "flex w-full items-center my-4"
        : "flex flex-col items-center self-stretch mx-4",
      this.inset() && this.isHorizontal() ? "px-4" : "",
    ]);
  }
}
