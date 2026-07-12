import { booleanAttribute, Component, computed, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import { classes } from "../../primitives/utils";
import type { UiBadgeTone } from "./badge-intent";
import { uiBadgeClass, type UiBadgeSize } from "./badge-variants";

/**
 * The tag / chip.
 *
 * For status-driven badges that resolve their own severity from a domain value, use
 * `<ui-status-badge>`; it wraps this one.
 *
 * =================================================================================================
 * THE `severity` DEFAULT STAYS `"info"`.
 * =================================================================================================
 * Every `<ui-badge>` that omits `severity` is info-blue on screen right now. Re-pointing this
 * default (at `primary`, say) would repaint every defaulted ui-badge while touching no call site
 * and failing no test — the classic wrapper-default-drift bug. The default is this component's
 * contract.
 * =================================================================================================
 *
 * WHY THE HOST IS THE CHIP (there is no inner element)
 * All 13 `class=` call sites lay out against the host box. `classes()` (Helm's, from
 * primitives/utils) writes our computed classes onto that same host and twMerges the call site's
 * `class` LAST — so `class="text-xs"` beats the `md` size cell instead of racing it in stylesheet
 * order. That is also why `class` needs no passthrough input.
 *
 * @example
 * <ui-badge value="Delivered" severity="success" />
 * <ui-badge [value]="count()" severity="danger" rounded icon="triangle-alert" />
 * <ui-badge severity="warn">Cancels at period end</ui-badge>
 */
@Component({
  selector: "ui-badge",
  templateUrl: "./badge.html",
  imports: [Icon],
})
export class Badge {
  /**
   * The chip's text. `null` / `undefined` / `""` render nothing and the projected `<ng-content>`
   * takes over.
   *
   * `undefined` is in the union because two call sites reach the status through optional chaining
   * (`load()?.status`), so they hand over `LoadStatus | undefined` and have always rendered an
   * empty chip when it is absent. Note what this forces on `text()` below: a strict
   * `value === null` check would let `undefined` through to `String(undefined)` and paint a chip
   * that literally reads "undefined".
   */
  public readonly value = input<string | number | null | undefined>(null);

  public readonly severity = input<UiBadgeTone>("info");

  public readonly size = input<UiBadgeSize>("md");

  /** Renders the chip as a fully-rounded pill. */
  public readonly rounded = input(false, { transform: booleanAttribute });

  /**
   * Typed against the generated `IconName` union — an unknown icon is a compile error, not a blank.
   *
   * `undefined` is accepted alongside `null`, and that is deliberate. "No icon" arrives here from
   * producers that spell absence the way TypeScript does — `icon?: IconName` on a lookup table
   * (truck-type-tag, load-type-tag) and `computed<IconName | undefined>` (trip-status-tag) — so
   * `IconName | null` alone would REJECT four call sites. The alternative was to force `?? null`
   * through four producers, i.e. to make them spell absence unnaturally so this input could be
   * narrower for no benefit. Both mean the same thing, and `@if (icon(); as name)` in the template
   * treats them identically.
   */
  public readonly icon = input<IconName | null | undefined>(null);

  protected readonly text = computed(() => {
    const value = this.value();
    // `== null` (loose) on purpose: it catches BOTH null and undefined. `=== null` would send
    // `undefined` to `String(undefined)` and render the word "undefined" in the chip.
    return value == null || value === "" ? null : String(value);
  });

  constructor() {
    // Writes onto the HOST, twMerging the call site's `class` last so a call site always wins.
    classes(() =>
      uiBadgeClass({ tone: this.severity(), size: this.size(), rounded: this.rounded() }),
    );
  }
}
