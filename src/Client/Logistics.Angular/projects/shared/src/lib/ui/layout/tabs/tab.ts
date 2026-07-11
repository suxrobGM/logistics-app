import { Component } from "@angular/core";
import { BrnTabsTrigger } from "@spartan-ng/brain/tabs";
import { TABS_TRIGGER_CLASSES } from "../../primitives/tabs";
import { classes } from "../../primitives/utils";

/**
 * A tab button. Replaces `<p-tab>`. The label — icons, count badges — is projected.
 *
 * `value` IS brain's `triggerFor`, exposed straight through the host directive, so it must be a plain
 * STRING: `<ui-tab value="0">`, not `[value]="0"`. A `hostDirectives` input cannot be run through a
 * transform, and brain's `triggerFor` is a required `string`. `ui-tabs` meanwhile still accepts a
 * numeric `[value]` and stringifies it, so `signal(0)` call sites keep working unchanged — the two
 * meet at `String(0) === "0"`.
 *
 * The trigger sits on the HOST, not on a `<button>` inside the template, and that is load-bearing:
 * `BrnTabsList` collects triggers with a CONTENT query, which does not descend into a child
 * component's view. Nesting the trigger would have handed it an empty list and silently killed
 * arrow-key navigation between tabs — mouse clicks would still have worked, so nothing would fail.
 *
 * Brain drives `role="tab"`, `aria-selected`, `aria-controls` and a roving `tabindex` on this host,
 * and activates on focus (`activationMode` defaults to `'automatic'`), which is the standard ARIA
 * automatic-activation tabs pattern.
 */
@Component({
  selector: "ui-tab",
  templateUrl: "./tab.html",
  hostDirectives: [{ directive: BrnTabsTrigger, inputs: ["brnTabsTrigger: value", "disabled"] }],
  host: { "data-slot": "tabs-trigger" },
})
export class UiTab {
  constructor() {
    classes(() => TABS_TRIGGER_CLASSES);
  }
}
