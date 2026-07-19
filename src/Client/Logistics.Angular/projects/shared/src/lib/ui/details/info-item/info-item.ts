import { Component, computed, input } from "@angular/core";
import { Typography } from "../../display/typography/typography";

/**
 * A stacked label / value block for detail cards - the muted-label-over-value pattern repeated all
 * over accident-details and the other detail pages. The label sits above the value as a small muted
 * caption; the value is emphasised body text.
 *
 * The value comes from either the `value` input or projected content, so the same component covers
 * plain text and richer markup (a link, a badge, a date pipe). Projection detection is kept simple
 * to stay robust: a consumer passes EITHER `value` (+ optional `emptyText`) OR projects content -
 * not both.
 * - Pass `value` for plain text; `emptyText` renders muted in its place when `value` is null/empty.
 * - Project content when you need markup; leave `value`/`emptyText` unset. (Projected content is
 *   the `<ng-content>` fallback path and only shows while `emptyText` is unset.)
 *
 * ```html
 * <ui-info-item label="Driver Name" [value]="party.driverName" emptyText="Not provided" />
 * <ui-info-item label="Truck">
 *   <a [routerLink]="['/trucks', id]" class="text-foreground font-medium">{{ number }}</a>
 * </ui-info-item>
 * ```
 */
@Component({
  selector: "ui-info-item",
  templateUrl: "./info-item.html",
  imports: [Typography],
  host: { class: "block" },
})
export class InfoItem {
  /** The field label, shown as a small muted caption above the value. */
  public readonly label = input.required<string>();

  /** Plain-text value. Ignored when content is projected. */
  public readonly value = input<string | number | null>(null);

  /** Muted placeholder shown when `value` is null/empty (opt-in: only renders if set). */
  public readonly emptyText = input<string | null>(null);

  protected readonly isEmpty = computed(() => {
    const value = this.value();
    return this.emptyText() !== null && (value === null || value === "");
  });
}
