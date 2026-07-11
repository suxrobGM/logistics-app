import { Directive } from "@angular/core";
import { BrnAccordion } from "@spartan-ng/brain/accordion";
import { classes } from "../../utils";

/** Lifted to a const so the `ui-*` wrapper can wear these classes without composing the DIRECTIVE (NG3001). */
export const ACCORDION_ROOT_CLASSES = "flex w-full flex-col";

@Directive({
  selector: "[hlmAccordion], hlm-accordion",
  hostDirectives: [{ directive: BrnAccordion, inputs: ["type", "orientation"] }],
  host: {
    "data-slot": "accordion",
  },
})
export class HlmAccordion {
  constructor() {
    classes(() => ACCORDION_ROOT_CLASSES);
  }
}
