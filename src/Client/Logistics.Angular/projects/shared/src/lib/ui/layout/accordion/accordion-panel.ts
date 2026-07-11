import { Component } from "@angular/core";
import { BrnAccordionItem } from "@spartan-ng/brain/accordion";
import { ACCORDION_ITEM_CLASSES } from "../../primitives/accordion";
import { classes } from "../../primitives/utils";

/**
 * One expandable panel. Replaces `<p-accordion-panel>`.
 *
 * PrimeNG's `[value]` key is gone: brain tracks open state per item instance (`openItem` / `closeItem`
 * through DI), so an author-supplied identity is redundant. Bind `[isOpened]` to open a panel
 * initially and `(openedChange)` to observe it.
 */
@Component({
  selector: "ui-accordion-panel",
  templateUrl: "./accordion-panel.html",
  hostDirectives: [
    { directive: BrnAccordionItem, inputs: ["isOpened", "disabled"], outputs: ["openedChange"] },
  ],
  host: { "data-slot": "accordion-item" },
})
export class UiAccordionPanel {
  constructor() {
    classes(() => ACCORDION_ITEM_CLASSES);
  }
}
