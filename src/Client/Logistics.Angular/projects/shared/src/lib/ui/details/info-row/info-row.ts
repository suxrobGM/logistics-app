import { booleanAttribute, Component, input } from "@angular/core";
import { Divider } from "../../layout/divider/divider";

/**
 * A horizontal label / value line for detail cards - the `label … value` row repeated across the
 * tms detail pages (truck-details, load-details, …), where each row is a flex `justify-between`
 * with a muted label on the left and an emphasised value on the right, optionally preceded by a
 * `<ui-divider>` to group sections.
 *
 * The value comes from either the `value` input or projected content. Projected content wins; if
 * none is projected, `value()` renders in its place (via `<ng-content>` fallback content). Pass a
 * value for plain text, or project markup (a link, a badge, a tag) for anything richer - not both.
 *
 * ```html
 * <ui-info-row label="Truck #" [value]="truck().number" />
 * <ui-info-row label="Status" divider>
 *   <ui-status-badge … />
 * </ui-info-row>
 * ```
 */
@Component({
  selector: "ui-info-row",
  templateUrl: "./info-row.html",
  imports: [Divider],
  host: { class: "block" },
})
export class InfoRow {
  /** The row label, shown muted on the left. */
  public readonly label = input.required<string>();

  /** Plain-text value shown on the right. Ignored when content is projected. */
  public readonly value = input<string | number | null>(null);

  /** Render a `<ui-divider>` above the row to separate groups. */
  public readonly divider = input(false, { transform: booleanAttribute });
}
