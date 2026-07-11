import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { NgIcon, provideIcons } from "@ng-icons/core";
import { lucideCheck } from "@ng-icons/lucide";
import { BrnAutocompleteItem } from "@spartan-ng/brain/autocomplete";
import { classes } from "../../utils";

@Component({
  selector: "hlm-autocomplete-item",
  imports: [NgIcon],
  providers: [provideIcons({ lucideCheck })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  hostDirectives: [{ directive: BrnAutocompleteItem, inputs: ["id", "disabled", "value"] }],
  host: { "data-slot": "autocomplete-item" },
  template: `
    <ng-content />
    @if (_active()) {
      <ng-icon
        name="lucideCheck"
        class="absolute end-2 flex items-center justify-center text-[length:--spacing(4)]"
        aria-hidden="true"
      />
    }
  `,
})
export class HlmAutocompleteItem {
  private readonly _brnAutocompleteItem = inject(BrnAutocompleteItem);

  protected readonly _active = this._brnAutocompleteItem.active;

  constructor() {
    classes(
      () =>
        "data-highlighted:bg-accent data-highlighted:text-accent-foreground not-data-[variant=destructive]:data-highlighted:**:text-accent-foreground gap-2 rounded-sm py-1.5 ps-2 pe-8 text-sm relative flex w-full cursor-default items-center outline-hidden select-none data-disabled:pointer-events-none data-disabled:opacity-50 data-hidden:hidden [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0",
    );
  }
}
