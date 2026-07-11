import { computed, Directive, effect, input, untracked } from "@angular/core";
import { injectCustomClassSettable } from "@spartan-ng/brain/core";
import { BrnSheetOverlay } from "@spartan-ng/brain/sheet";
import type { ClassValue } from "clsx";
import { hlm } from "../../utils";

@Directive({
  selector: "[hlmSheetOverlay],hlm-sheet-overlay",
  hostDirectives: [BrnSheetOverlay],
})
export class HlmSheetOverlay {
  private readonly _classSettable = injectCustomClassSettable({ optional: true, host: true });
  public readonly userClass = input<ClassValue>("", { alias: "class" });
  protected readonly _computedClass = computed(() =>
    hlm(
      "data-open:animate-in data-closed:animate-out data-closed:fade-out-0 data-open:fade-in-0 isolate bg-black/10 duration-100 supports-backdrop-filter:backdrop-blur-xs",
      this.userClass(),
    ),
  );

  constructor() {
    effect(() => {
      const classValue = this._computedClass();
      untracked(() => this._classSettable?.setClassToCustomElement(classValue));
    });
  }
}
