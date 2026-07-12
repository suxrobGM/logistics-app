import { Component } from "@angular/core";
import { BrnTabsContent } from "@spartan-ng/brain/tabs";
import { TABS_CONTENT_CLASSES } from "../../primitives/tabs";
import { classes } from "../../primitives/utils";

/**
 * One tab's content.
 *
 * `value` is brain's `contentFor` straight through the host directive, so — as for `ui-tab` — it is a
 * plain string attribute: `<ui-tab-panel value="0">`.
 *
 * Brain hides the unselected panel with `[hidden]` on this host and sets `role="tabpanel"` /
 * `aria-labelledby`, so no inner wrapper is needed — and not adding one keeps the panel's own box out
 * of the layout.
 */
@Component({
  selector: "ui-tab-panel",
  templateUrl: "./tab-panel.html",
  hostDirectives: [{ directive: BrnTabsContent, inputs: ["brnTabsContent: value"] }],
  host: { "data-slot": "tabs-content" },
})
export class UiTabPanel {
  constructor() {
    classes(() => TABS_CONTENT_CLASSES);
  }
}
