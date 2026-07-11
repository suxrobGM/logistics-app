import { type BooleanInput } from "@angular/cdk/coercion";
import { CdkMenuItem, CdkMenuItemCheckbox, CdkMenuItemSelectable } from "@angular/cdk/menu";
import { booleanAttribute, Directive, inject, input } from "@angular/core";
import { classes } from "../../utils";
import { HlmDropdownMenuFocusOnHover } from "./hlm-dropdown-menu-focus-on-hover";

/** @internal. Use HlmDropdownMenuCheckbox instead. */
@Directive({
  selector: "[hlmDropdownMenuCheckboxCdk]",
  providers: [
    { provide: CdkMenuItemCheckbox, useExisting: HlmDropdownMenuCheckboxCdk },
    { provide: CdkMenuItemSelectable, useExisting: HlmDropdownMenuCheckboxCdk },
    { provide: CdkMenuItem, useExisting: CdkMenuItemSelectable },
  ],
})
export class HlmDropdownMenuCheckboxCdk extends CdkMenuItemCheckbox {
  public readonly keepOpen = input<boolean, BooleanInput>(true, { transform: booleanAttribute });

  public override trigger(options?: { keepOpen: boolean }) {
    super.trigger({ ...options, keepOpen: this.keepOpen() });
  }
}

@Directive({
  selector: "[hlmDropdownMenuCheckbox],[hlmDropdownMenuCheckboxItem]",
  hostDirectives: [
    {
      directive: HlmDropdownMenuCheckboxCdk,
      inputs: ["cdkMenuItemDisabled: disabled", "cdkMenuItemChecked: checked", "keepOpen"],
      outputs: ["cdkMenuItemTriggered: triggered"],
    },
    HlmDropdownMenuFocusOnHover,
  ],
  host: {
    "data-slot": "dropdown-menu-checkbox-item",
    "[attr.data-disabled]": '_cdkMenuItem.disabled ? "" : null',
    "[attr.data-checked]": '_cdkMenuItem.checked ? "" : null',
    "[attr.data-inset]": 'inset() ? "" : null',
  },
})
export class HlmDropdownMenuCheckbox {
  protected readonly _cdkMenuItem = inject(HlmDropdownMenuCheckboxCdk);

  public readonly inset = input<boolean, BooleanInput>(false, {
    transform: booleanAttribute,
  });

  constructor() {
    classes(
      () =>
        "hover:bg-accent focus:bg-accent hover:text-accent-foreground focus:text-accent-foreground gap-2 rounded-sm py-1.5 ps-2 pe-8 text-sm data-inset:ps-8 [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(4)] group/dropdown-menu-checkbox relative flex w-full cursor-default items-center outline-hidden select-none data-disabled:pointer-events-none data-disabled:opacity-50 [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0",
    );
  }
}
