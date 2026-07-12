import { Component, signal } from "@angular/core";
import {
  Icon,
  Typography,
  UiButton,
  UiTooltip,
  type UiTooltipPosition,
} from "@logistics/shared/ui";

/**
 * The S6 tooltip: the `[uiTooltip]` directive.
 *
 * The rows here exist to make the FOUR things a build and a test run both stay green through
 * visible in a browser in five seconds:
 *
 *   1. IT OPENS ON KEYBOARD FOCUS, THROUGH THE `<ui-button>` WRAPPER. 82 of the 124 migrated hosts
 *      are `<ui-button>`, which renders its real `<button>` inside itself. `focus` does not bubble,
 *      so the natural implementation (a directive that listens on its own host — which is what
 *      spartan's `BrnTooltip` does) NEVER FIRES on those. Tab into the icon row below: every one of
 *      them must show its tooltip. This is the row that would have shipped broken.
 *   2. IT CLOSES. On mouseout, on blur, and on Escape. A tooltip stuck open is worse than none.
 *   3. IT DOES NOT BECOME THE ACCESSIBLE NAME. Every icon-only button below has an explicit
 *      `ariaLabel`; the tooltip must add `aria-describedby` to the inner `<button>` and touch
 *      nothing else.
 *   4. AN EMPTY TOOLTIP IS NO TOOLTIP, not an empty box — 38 of the 124 hosts bind their text, and
 *      a bound value goes empty all the time.
 */
@Component({
  selector: "app-ui-lab-tooltip",
  templateUrl: "./tooltip-section.html",
  imports: [Icon, Typography, UiButton, UiTooltip],
})
export class UiLabTooltipSection {
  protected readonly positions: readonly UiTooltipPosition[] = ["top", "bottom", "left", "right"];

  /** Bound text, so the "goes empty -> no tooltip" case is drivable from the page. */
  protected readonly boundText = signal<string>("I am bound, and I can go empty");

  protected readonly toggleBoundText = () =>
    this.boundText.update((t) => (t ? "" : "I am bound, and I can go empty"));

  /** The icon-only row — the shape 82 of the 124 real call sites have. */
  protected readonly iconButtons = [
    { icon: "pencil", label: "Edit load" },
    { icon: "trash", label: "Delete load" },
    { icon: "refresh-cw", label: "Refresh exceptions" },
    { icon: "eye", label: "View session" },
    { icon: "download", label: "Download manifest" },
  ] as const;
}
