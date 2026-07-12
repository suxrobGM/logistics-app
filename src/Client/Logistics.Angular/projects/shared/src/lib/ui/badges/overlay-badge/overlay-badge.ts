import { Component, computed, input } from "@angular/core";
import type { UiBadgeIntent } from "../badge/badge-intent";
import { CountBadge, type UiCountBadgeSize } from "../count-badge/count-badge";

/**
 * A count pill pinned to the top-right corner of whatever you project into it (the notification bell).
 *
 * A null, empty or zero `value` must hide the badge entirely: callers say "nothing to report" by
 * handing over `null` and trust the badge to disappear, so rendering an empty dot instead would leave
 * the bell claiming unread notifications forever. A spec pins it.
 */
@Component({
  selector: "ui-overlay-badge",
  templateUrl: "./overlay-badge.html",
  imports: [CountBadge],
  host: {
    // Positioning context for the absolutely-placed pill. `inline-flex` keeps the projected content's
    // own box intact — the bell is an icon inside a button and must not become a block.
    class: "relative inline-flex",
  },
})
export class OverlayBadge {
  public readonly value = input<string | number | null>(null);
  public readonly severity = input<UiBadgeIntent>("danger");
  public readonly badgeSize = input<UiCountBadgeSize>("normal");

  /** Empty, null, or a zero count — all three mean "nothing to report", so nothing is drawn. */
  protected readonly hidden = computed(() => {
    const value = this.value();
    return value === null || value === "" || value === 0;
  });
}
