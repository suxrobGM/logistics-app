import { Directive } from "@angular/core";
import { BrnAutocompleteContent } from "@spartan-ng/brain/autocomplete";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmAutocompleteContent],hlm-autocomplete-content",
  hostDirectives: [BrnAutocompleteContent],
})
export class HlmAutocompleteContent {
  constructor() {
    classes(
      () =>
        "bg-popover text-popover-foreground data-open:animate-in data-closed:animate-out data-closed:fade-out-0 data-open:fade-in-0 data-closed:zoom-out-95 data-open:zoom-in-95 data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2 ring-foreground/10 max-h-72 min-w-36 overflow-hidden rounded-md shadow-md ring-1 duration-100 group/autocomplete-content flex w-(--brn-autocomplete-width) flex-col p-0",
    );
  }
}
