import { Directive, ElementRef, inject } from "@angular/core";
import { FormRoot } from "@angular/forms/signals";

/**
 * Reveals validation feedback when an invalid form is submitted.
 *
 * Auto-applies to every `<form [formRoot]>` in a component that imports it — no template attribute
 * and no submit-handler changes are required. On an invalid submit it:
 *  - scrolls to / focuses the first invalid control, and
 *  - announces the invalid-field count to assistive technologies via an `aria-live` region.
 *
 * It deliberately does NOT mark controls as touched: Signal Forms' own `submit()` already marks the
 * entire tree touched *before* it checks validity, so inline `ui-form-field` errors reveal
 * themselves. (Pinned by `signal-forms-v22-api-probe.spec.ts`, claim F.)
 *
 * Focusing relies on each control implementing the optional `focus()` hook of `FormUiControl` —
 * without it, `focusBoundControl()` falls back to `.focus()` on the wrapper's non-focusable
 * custom-element host and silently does nothing. (Claim L.)
 *
 * Note it cannot query `.ng-invalid`: Signal Forms applies no status classes by default. (Claim I.)
 */
@Directive({
  // Intentionally matches every signal form so the behavior is opt-in by import only.
  // eslint-disable-next-line @angular-eslint/directive-selector
  selector: "form[formRoot]",
  host: {
    "(submit)": "onSubmit()",
  },
})
export class ValidatedForm {
  private readonly formRoot = inject(FormRoot);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private liveRegion?: HTMLElement;

  protected onSubmit(): void {
    const state = this.formRoot.fieldTree()();
    if (!state.invalid()) {
      return;
    }

    // `errorSummary()` is the field's own errors plus every descendant's, in tree order. One field
    // can contribute several errors, so count distinct fields, not errors.
    const errors = state.errorSummary();
    const fields = new Set(errors.map((error) => error.fieldTree));
    this.announce(fields.size);
    errors[0]?.fieldTree().focusBoundControl();
  }

  private announce(invalidFieldCount: number): void {
    const message =
      invalidFieldCount === 1
        ? "1 field needs your attention before submitting."
        : `${invalidFieldCount} fields need your attention before submitting.`;

    if (!this.liveRegion) {
      const region = document.createElement("div");
      region.setAttribute("aria-live", "assertive");
      region.setAttribute("role", "status");
      // Visually hidden, still announced by screen readers.
      region.style.cssText =
        "position:absolute;width:1px;height:1px;margin:-1px;padding:0;overflow:hidden;clip:rect(0 0 0 0);white-space:nowrap;border:0;";
      this.host.nativeElement.appendChild(region);
      this.liveRegion = region;
    }
    // Clear first so repeated submits re-announce the same message.
    this.liveRegion.textContent = "";
    this.liveRegion.textContent = message;
  }
}
