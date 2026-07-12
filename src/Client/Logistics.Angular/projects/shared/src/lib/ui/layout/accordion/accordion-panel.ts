import { Component } from "@angular/core";
import { BrnAccordionItem } from "@spartan-ng/brain/accordion";
import { ACCORDION_ITEM_CLASSES } from "../../primitives/accordion";
import { classes } from "../../primitives/utils";

/**
 * One expandable panel.
 *
 * There is deliberately no author-supplied `[value]` key: brain tracks open state per item instance
 * (`openItem` / `closeItem` through DI), so an identity input would be redundant. Bind `[isOpened]`
 * to open a panel initially and `(openedChange)` to observe it.
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
