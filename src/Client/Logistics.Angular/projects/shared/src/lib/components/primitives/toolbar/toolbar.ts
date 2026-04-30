import { booleanAttribute, Component, computed, input } from "@angular/core";

/**
 * Page action bar above tables. Project content into named slots:
 * `slot="start"` (filters/search), `slot="center"` (title), `slot="end"` (primary actions).
 */
@Component({
  selector: "ui-toolbar",
  templateUrl: "./toolbar.html",
})
export class Toolbar {
  public readonly sticky = input<boolean, unknown>(false, { transform: booleanAttribute });

  protected readonly classes = computed(() => {
    const base = "mb-4 flex flex-wrap items-center gap-3";
    return this.sticky() ? `${base} sticky top-0 z-10 bg-base/80 backdrop-blur` : base;
  });
}
