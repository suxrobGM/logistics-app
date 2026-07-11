import { Directive } from "@angular/core";
import { BrnAccordionItem } from "@spartan-ng/brain/accordion";
import { classes } from "../../utils";

/** Lifted to a const so the `ui-*` wrapper can wear these classes without composing the DIRECTIVE (NG3001). */
export const ACCORDION_ITEM_CLASSES = "not-last:border-b flex flex-col";

@Directive({
  selector: "[hlmAccordionItem],hlm-accordion-item",
  hostDirectives: [
    {
      directive: BrnAccordionItem,
      inputs: ["isOpened", "disabled"],
      outputs: ["openedChange"],
    },
  ],
  host: {
    "data-slot": "accordion-item",
  },
})
export class HlmAccordionItem {
  constructor() {
    classes(() => ACCORDION_ITEM_CLASSES);
  }
}
