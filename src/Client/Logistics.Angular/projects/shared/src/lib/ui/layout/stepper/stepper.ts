import { Component, Directive, inject, input, output, TemplateRef } from "@angular/core";

/**
 * Marks the lazy body of a `<ui-step-panel>`:
 *
 *   <ui-step-panel [value]="2">
 *     <ng-template uiStepContent>…</ng-template>
 *   </ui-step-panel>
 *
 * A template, not plain projection, and that is deliberate. Content passed through `<ng-content>` is
 * instantiated with the PARENT view whether or not the slot is rendered, so every step of every wizard
 * would be constructed up front — `trip-wizard` step 2 alone is a full loads table, and
 * `accident-add`'s steps are forms that would start validating before they were ever shown. The
 * template keeps every step lazy.
 */
@Directive({ selector: "[uiStepContent]" })
export class UiStepContent {
  public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}

/**
 * A linear wizard, with 3 call sites (trip-wizard, accident-add, accident-edit) — all of them
 * `[linear]="true"` with numeric step values.
 *
 * Hand-rolled: spartan has no stepper. It is deliberately dumb — it renders headers and swaps panels,
 * and that is all. Every call site already drives navigation from its own Back/Next buttons through
 * its own store, so this owns no navigation logic of its own.
 *
 * `linear` blocks only FORWARD jumps from the header (`value > active`). Going back to a completed
 * step stays allowed, which is what the wizards' Back buttons rely on.
 */
@Component({
  selector: "ui-stepper",
  templateUrl: "./stepper.html",
})
export class UiStepper {
  public readonly value = input<number>(1);
  public readonly linear = input(false);
  public readonly valueChange = output<number>();

  /** Called by `ui-step` headers. Forward jumps are already filtered out there when `linear`. */
  public select(next: number): void {
    if (next === this.value()) return;
    this.valueChange.emit(next);
  }
}

/** The header strip. */
@Component({
  selector: "ui-step-list",
  templateUrl: "./step-list.html",
  host: { class: "flex items-center gap-2", role: "tablist" },
})
export class UiStepList {}

/** The panel area. */
@Component({
  selector: "ui-step-panels",
  templateUrl: "./step-panels.html",
  host: { class: "block pt-6" },
})
export class UiStepPanels {}
