import { NgTemplateOutlet } from "@angular/common";
import { Component, computed, contentChild, inject, input } from "@angular/core";
import { UiStepContent, UiStepper } from "./stepper";

/**
 * One step's body. Replaces `<p-step-panel>`.
 *
 * Renders only while its step is active, and renders from a `<ng-template uiStepContent>` so the body
 * is not constructed until then — see `UiStepContent` for why laziness is load-bearing here.
 */
@Component({
  selector: "ui-step-panel",
  templateUrl: "./step-panel.html",
  imports: [NgTemplateOutlet],
})
export class UiStepPanel {
  private readonly stepper = inject(UiStepper);

  public readonly value = input.required<number>();

  protected readonly content = contentChild.required(UiStepContent);
  protected readonly active = computed(() => this.stepper.value() === this.value());
}
