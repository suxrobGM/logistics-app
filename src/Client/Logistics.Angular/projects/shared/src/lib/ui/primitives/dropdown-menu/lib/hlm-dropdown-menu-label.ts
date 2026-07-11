import { type BooleanInput } from "@angular/cdk/coercion";
import { booleanAttribute, Directive, input } from "@angular/core";
import { classes } from "../../utils";

@Directive({
  selector: "[hlmDropdownMenuLabel],hlm-dropdown-menu-label",
  host: {
    "data-slot": "dropdown-menu-label",
    "[attr.data-inset]": 'inset() ? "" : null',
  },
})
export class HlmDropdownMenuLabel {
  public readonly inset = input<boolean, BooleanInput>(false, {
    transform: booleanAttribute,
  });

  constructor() {
    classes(() => "text-muted-foreground px-2 py-1.5 text-xs font-medium data-inset:ps-8 block");
  }
}
