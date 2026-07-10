import { booleanAttribute, Component, computed, input } from "@angular/core";

export type DividerOrientation = "horizontal" | "vertical";

/**
 * Horizontal or vertical separator with optional inline label via `<ng-content>`.
 */
@Component({
  selector: "ui-divider",
  templateUrl: "./divider.html",
})
export class Divider {
  public readonly orientation = input<DividerOrientation>("horizontal");
  public readonly inset = input<boolean, unknown>(false, { transform: booleanAttribute });

  protected readonly isHorizontal = computed(() => this.orientation() === "horizontal");

  protected readonly wrapperClasses = computed(() => {
    const base = this.isHorizontal()
      ? "flex items-center w-full my-3"
      : "flex flex-col items-center self-stretch mx-3";
    return this.inset() && this.isHorizontal() ? `${base} px-4` : base;
  });

  protected readonly lineClasses = computed(() =>
    this.isHorizontal() ? "flex-1 border-t border-default" : "flex-1 border-l border-default",
  );
}
