import { Component, input } from "@angular/core";
import { Card } from "../../containers/card/card";
import { Typography } from "../../display/typography/typography";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";

/**
 * A titled detail card - the card-with-header boilerplate repeated across ~13 tms detail pages
 * (truck-details, accident-details, load/trip/employee detail, …). Renders a `<ui-card>` with a
 * `#header` slot holding the heading (and an optional leading glyph), plus an optional actions
 * slot on the right and the body via the default `<ng-content>`.
 *
 * The `<ng-template #header>` is a DIRECT child of `<ui-card>` on purpose: the card queries its
 * header slot with `descendants: false`, so a header template nested any deeper is ignored (see
 * `containers/card/card.ts`).
 *
 * Uses `border-border` (cross-portal-safe), not the TMS-only `border-default` the raw pages spell
 * out, so the component reads identically in admin / customer / tms.
 *
 * ```html
 * <ui-detail-section heading="Truck Information" icon="truck">
 *   <ui-button uiDetailSectionActions appearance="text" label="Edit" />
 *   <ui-stack gap="4"> … rows … </ui-stack>
 * </ui-detail-section>
 * ```
 */
@Component({
  selector: "ui-detail-section",
  templateUrl: "./detail-section.html",
  imports: [Card, Typography, Icon],
})
export class DetailSection {
  /** Section heading, rendered as an `h5`/`h3` inside the card header. */
  public readonly heading = input.required<string>();

  /** Optional leading glyph shown before the heading. */
  public readonly icon = input<IconName | null>(null);
}
