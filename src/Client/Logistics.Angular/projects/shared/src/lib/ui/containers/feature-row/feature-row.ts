import { booleanAttribute, Component, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import { UiTooltip } from "../../overlay/tooltip/tooltip";

/**
 * A single row in a feature-settings list: feature name, optional description,
 * an optional "locked" state (dims the row and shows a lock glyph), and a slot
 * for trailing controls (toggles, checkboxes) projected via `<ng-content>`.
 *
 * Shared by the admin portal (default + per-tenant feature config) and the TMS
 * portal (tenant self-service feature toggles) so all three read identically.
 * Uses only cross-portal-safe utilities (`border-border`, `text-muted-foreground`),
 * never the TMS-only `border-default` / `bg-subtle` tokens.
 */
@Component({
  selector: "ui-feature-row",
  templateUrl: "./feature-row.html",
  imports: [Icon, UiTooltip],
})
export class FeatureRow {
  public readonly name = input.required<string>();
  public readonly description = input<string | null>(null);
  public readonly locked = input<boolean, unknown>(false, { transform: booleanAttribute });
  public readonly lockedTooltip = input<string>("Locked by the administrator");
}
