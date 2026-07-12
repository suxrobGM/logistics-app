import { Component } from "@angular/core";
import { BrnAccordion } from "@spartan-ng/brain/accordion";
import { ACCORDION_ROOT_CLASSES } from "../../primitives/accordion";
import { classes } from "../../primitives/utils";

/**
 * Accordion, with 3 call sites.
 *
 * ONE CHILD API, deliberately: element children only, no `<ng-template #header>` / `#content`
 * slots. Rather than carry a dual-API component forever, the two template-driven sites (the
 * "Danger Zone" panels in the customer and employee edit dialogs) were hand-converted to element
 * children. One shape, no branching.
 *
 * `type` is `"single"` (default) or `"multiple"`.
 *
 * Hosts brain's `BrnAccordion` rather than `HlmAccordion`, because a Helm host directive would drag
 * the private primitive into the public API (NG3001); Helm's classes come across as a const instead.
 * Panels resolve the root through DI, and brain's own `contentChildren` query feeds only the
 * `openAll()`/`closeAll()` helpers we do not expose — which is why the FAQ's panels still work
 * despite sitting inside an `@for` (an embedded view a `descendants: false` query cannot reach).
 */
@Component({
  selector: "ui-accordion",
  templateUrl: "./accordion.html",
  hostDirectives: [{ directive: BrnAccordion, inputs: ["type", "orientation"] }],
  host: { "data-slot": "accordion" },
})
export class UiAccordion {
  constructor() {
    classes(() => ACCORDION_ROOT_CLASSES);
  }
}
