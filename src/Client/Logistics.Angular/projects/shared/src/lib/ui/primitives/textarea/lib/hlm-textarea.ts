import { Directive } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmTextarea]",
  host: { "data-slot": "textarea" },
})
export class HlmTextarea {
  constructor() {
    classes(
      () =>
        "border-sp-input dark:bg-sp-input/30 focus-visible:border-sp-ring focus-visible:ring-sp-ring/50 aria-[invalid=true]:ring-sp-destructive/20 dark:aria-[invalid=true]:ring-sp-destructive/40 aria-[invalid=true]:border-sp-destructive dark:aria-[invalid=true]:border-sp-destructive/50 disabled:bg-sp-input/50 dark:disabled:bg-sp-input/80 rounded-lg border bg-transparent px-2.5 py-2 text-base transition-colors focus-visible:ring-3 aria-[invalid=true]:ring-3 md:text-sm placeholder:text-sp-muted-foreground flex field-sizing-content min-h-16 w-full outline-none disabled:cursor-not-allowed disabled:opacity-50",
    );
  }
}
