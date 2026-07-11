import { Directive } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmPopoverHeader],hlm-popover-header",
  host: { "data-slot": "popover-header" },
})
export class HlmPopoverHeader {
  constructor() {
    classes(() => "flex flex-col gap-1 text-sm");
  }
}
