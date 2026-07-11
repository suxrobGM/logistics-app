import { Component } from "@angular/core";
import { Typography } from "@logistics/shared/ui";
import { UiLabFormShowcase, type LabFormVariant } from "./form-showcase";

/**
 * The same form — every `ui-*-field` that exists — rendered three times, once per state. Side by
 * side is the point: pristine vs invalid vs disabled is where a wrapper's default drifts, and a
 * drifted default is only obvious next to the version that did not drift.
 */
@Component({
  selector: "app-ui-lab-forms",
  templateUrl: "./forms-section.html",
  imports: [Typography, UiLabFormShowcase],
})
export class UiLabFormsSection {
  protected readonly variants: readonly LabFormVariant[] = ["pristine", "invalid", "disabled"];
}
