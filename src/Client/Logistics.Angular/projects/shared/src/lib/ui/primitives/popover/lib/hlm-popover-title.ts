import { Directive } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmPopoverTitle]",
  host: { "data-slot": "popover-title" },
})
export class HlmPopoverTitle {
  constructor() {
    classes(() => "font-medium");
  }
}
