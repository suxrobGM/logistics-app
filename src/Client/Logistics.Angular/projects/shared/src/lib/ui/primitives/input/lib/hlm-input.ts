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
        "dark:bg-input/30 border-input focus-visible:border-ring focus-visible:ring-ring/50 aria-[invalid=true]:ring-destructive/20 dark:aria-[invalid=true]:ring-destructive/40 aria-[invalid=true]:border-destructive dark:aria-[invalid=true]:border-destructive/50 disabled:bg-input/50 dark:disabled:bg-input/80 h-8 rounded-lg border bg-transparent px-2.5 py-1 text-base transition-colors file:h-6 file:text-sm file:font-medium focus-visible:ring-3 aria-[invalid=true]:ring-3 md:text-sm file:text-foreground placeholder:text-muted-foreground w-full min-w-0 outline-none file:inline-flex file:border-0 file:bg-transparent disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50",
    );
  }
}
