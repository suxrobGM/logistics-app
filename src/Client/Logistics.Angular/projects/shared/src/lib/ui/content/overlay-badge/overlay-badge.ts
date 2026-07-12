import { Component, computed, input } from "@angular/core";
import type { UiBadgeTone } from "../badge/badge-intent";
import { CountBadge, type UiCountBadgeSize } from "../count-badge/count-badge";

/**
 * A count pill pinned to the top-right corner of whatever you project into it. One call site: the
 * notification bell.
 *
 * =================================================================================================
 * A NULL / EMPTY `value` MUST HIDE THE BADGE ENTIRELY. This is the whole component.
 * =================================================================================================
 * The bell binds:
 *
 *     [value]="unreadCount() > 0 ? (unreadCount() > 9 ? '9+' : unreadCount().toString()) : null"
 *
 * i.e. it says "no unread messages" by handing over `null` and trusts the badge to disappear —
 * nothing at the call site (no `@if`, no class, no comment) records that the dependency exists.
 * Render an empty red dot on `null` and the bell claims unread notifications forever, on every
 * page, for every user. There is a spec pinning this precisely because the call site cannot tell
 * you it matters.
 *
 * `hidden()` also covers `""` and `0`: `0` unread is not a notification either.
 */
@Component({
  selector: "ui-overlay-badge",
  templateUrl: "./overlay-badge.html",
  imports: [CountBadge],
  host: {
    // The positioning context for the absolutely-placed pill. `inline-flex` keeps the projected
    // content's own box intact — the bell is an icon inside a button and must not become a block.
    class: "relative inline-flex",
  },
})
export class OverlayBadge {
  public readonly value = input<string | number | null>(null);
  public readonly severity = input<UiBadgeTone>("danger");
  public readonly badgeSize = input<UiCountBadgeSize>("normal");

  /** Empty, null, or a zero count — all three mean "nothing to report", so nothing is drawn. */
  protected readonly hidden = computed(() => {
    const value = this.value();
    return value === null || value === "" || value === 0;
  });
}
