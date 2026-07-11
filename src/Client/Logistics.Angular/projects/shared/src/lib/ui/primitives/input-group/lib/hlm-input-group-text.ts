import { Directive } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmInputGroupText],hlm-input-group-text",
})
export class HlmInputGroupText {
  constructor() {
    classes(
      () =>
        "text-muted-foreground gap-2 text-sm [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(4)] flex items-center [&_ng-icon]:pointer-events-none",
    );
  }
}
