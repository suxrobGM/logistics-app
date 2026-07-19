import { Component, computed, input } from "@angular/core";
import { Typography } from "../../display/typography/typography";

export type StatIntent = "default" | "success" | "danger" | "warning";

const INTENT_CLASSES: Record<StatIntent, string> = {
  default: "text-foreground",
  success: "text-success",
  danger: "text-danger",
  warning: "text-warning",
};

/**
 * A single statistic cell - the "Performance Summary" stat pattern in truck-details: a small muted
 * caption label above a bold value, with the value optionally tinted by intent (e.g. green gross,
 * red overrun). Meant to sit in a grid of these.
 *
 * ```html
 * <ui-stat-item label="Gross (30 days)" [value]="gross | currencyFormat" intent="success" />
 * <ui-stat-item label="RPM (30 days)" [value]="rpm | currencyFormat" />
 * ```
 */
@Component({
  selector: "ui-stat-item",
  templateUrl: "./stat-item.html",
  imports: [Typography],
  host: { class: "block" },
})
export class StatItem {
  /** Caption label shown above the value. */
  public readonly label = input.required<string>();

  /** The formatted statistic value. */
  public readonly value = input.required<string>();

  /** Tints the value: neutral foreground, or a success / danger / warning semantic colour. */
  public readonly intent = input<StatIntent>("default");

  protected readonly intentClass = computed(() => INTENT_CLASSES[this.intent()]);
}
