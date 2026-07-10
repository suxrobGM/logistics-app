import { Directive } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmInput]",
  host: { "data-slot": "input" },
})
export class HlmInput {
  constructor() {
    classes(
      () =>
        "dark:bg-sp-input/30 border-sp-input focus-visible:border-sp-ring focus-visible:ring-sp-ring/50 aria-[invalid=true]:ring-sp-destructive/20 dark:aria-[invalid=true]:ring-sp-destructive/40 aria-[invalid=true]:border-sp-destructive dark:aria-[invalid=true]:border-sp-destructive/50 disabled:bg-sp-input/50 dark:disabled:bg-sp-input/80 h-8 rounded-lg border bg-transparent px-2.5 py-1 text-base transition-colors file:h-6 file:text-sm file:font-medium focus-visible:ring-3 aria-[invalid=true]:ring-3 md:text-sm file:text-sp-foreground placeholder:text-sp-muted-foreground w-full min-w-0 outline-none file:inline-flex file:border-0 file:bg-transparent disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50",
    );
  }
}
