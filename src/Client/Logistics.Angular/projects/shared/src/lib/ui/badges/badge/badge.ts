import { booleanAttribute, Component, computed, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import { classes } from "../../primitives/utils";
import type { UiBadgeIntent } from "./badge-intent";
import { uiBadgeClass, type UiBadgeSize } from "./badge-variants";

/**
 * The tag / chip. For status-driven badges that resolve their own severity from a domain value, use
 * `<ui-status-badge>`; it wraps this one.
 *
 * The `severity` default of `"info"` is part of the contract — repointing it would repaint every
 * defaulted badge without touching a call site or failing a test. The host IS the chip (no inner
 * element): `classes()` writes onto the host and twMerges the call site's `class` last, so a call
 * site's `text-xs` beats the size cell rather than racing it in stylesheet order.
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
   * takes over. `undefined` is in the union because callers reach the value through optional
   * chaining (`load()?.status`).
   */
  public readonly value = input<string | number | null | undefined>(null);

  public readonly severity = input<UiBadgeIntent>("info");

  public readonly size = input<UiBadgeSize>("md");

  /** Renders the chip as a fully-rounded pill. */
  public readonly rounded = input(false, { transform: booleanAttribute });

  /**
   * Typed against the generated `IconName` union — an unknown icon is a compile error, not a blank.
   * `undefined` is accepted alongside `null` so producers that spell absence as `icon?: IconName` or
   * `computed<IconName | undefined>` need no `?? null`; the template treats both the same.
   */
  public readonly icon = input<IconName | null | undefined>(null);

  protected readonly text = computed(() => {
    const value = this.value();
    // Loose `== null` on purpose: it catches undefined too, which would otherwise reach
    // `String(undefined)` and render the word "undefined" in the chip.
    return value == null || value === "" ? null : String(value);
  });

  constructor() {
    // Writes onto the HOST, twMerging the call site's `class` last so a call site always wins.
    classes(() =>
      uiBadgeClass({ tone: this.severity(), size: this.size(), rounded: this.rounded() }),
    );
  }
}
