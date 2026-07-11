import { Directive } from "@angular/core";
import { BrnAutocompleteList } from "@spartan-ng/brain/autocomplete";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmAutocompleteList]",
  hostDirectives: [{ directive: BrnAutocompleteList, inputs: ["id"] }],
  host: { "data-slot": "autocomplete-list" },
})
export class HlmAutocompleteList {
  constructor() {
    classes(
      () =>
        "no-scrollbar max-h-[calc(--spacing(72)---spacing(9))] scroll-py-1 p-1 data-empty:p-0 overflow-y-auto overscroll-contain",
    );
  }
}
