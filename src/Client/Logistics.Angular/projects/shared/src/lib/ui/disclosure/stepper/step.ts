import { Component, computed, inject, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import { UiStepper } from "./stepper";

/**
 * One step header. Replaces `<p-step>`.
 *
 * Resolves its `UiStepper` through DI: `<ui-step>` is declared inside `<ui-stepper>` in the caller's
 * template, so its element injector is parented there.
 */
@Component({
  selector: "ui-step",
  templateUrl: "./step.html",
  imports: [Icon],
  // `group` so the connector span inside can hide itself on the last step via `group-last:hidden`.
  host: { class: "group flex flex-1 items-center gap-2 last:flex-none" },
})
export class UiStep {
  private readonly stepper = inject(UiStepper);

  public readonly value = input.required<number>();

  protected readonly active = computed(() => this.stepper.value() === this.value());
  protected readonly completed = computed(() => this.value() < this.stepper.value());

  /** `linear` forbids skipping ahead; stepping BACK to a completed step stays allowed. */
  protected readonly disabled = computed(
    () => this.stepper.linear() && this.value() > this.stepper.value(),
  );

  protected select(): void {
    if (this.disabled()) return;
    this.stepper.select(this.value());
  }
}
