import { DestroyRef, Directive, ElementRef, inject, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { AbstractControl, FormGroupDirective } from "@angular/forms";
import { FormRoot } from "@angular/forms/signals";

/**
 * Reveals validation feedback when an invalid form is submitted.
 *
 * Auto-applies to every `<form [formRoot]>` (Signal Forms) and every `<form [formGroup]>` (legacy
 * Reactive Forms) in a component that imports it — no template attribute and no submit-handler
 * changes are required. On an invalid submit it:
 *  - scrolls to / focuses the first invalid control, and
 *  - announces the error count to assistive technologies via an `aria-live` region.
 *
 * ## Signal Forms
 *
 * It deliberately does NOT mark controls as touched: Signal Forms' own `submit()` already marks the
 * entire tree touched *before* it checks validity, so inline `ui-form-field` errors reveal
 * themselves. (Pinned by `signal-forms-v22-api-probe.spec.ts`, claim F.)
 *
 * Focusing relies on each `ui-*-field` wrapper implementing the optional `focus()` hook of
 * `FormUiControl` — without it, `focusBoundControl()` falls back to `.focus()` on the wrapper's
 * non-focusable custom-element host and silently does nothing. (Claim L.)
 *
 * Note it cannot query `.ng-invalid`: Signal Forms applies no status classes by default. (Claim I.)
 *
 * ## Reactive Forms — transitional
 *
 * The `form[formGroup]` half exists only while forms are being migrated to Signal Forms one at a
 * time. It is deleted, with the selector, at the end of Phase 4.
 */
@Directive({
  // Intentionally matches every form so the behavior is opt-in by import only.
  // eslint-disable-next-line @angular-eslint/directive-selector
  selector: "form[formRoot], form[formGroup]",
  host: {
    "(submit)": "onSignalSubmit()",
  },
})
export class ValidatedForm implements OnInit {
  private readonly formRoot = inject(FormRoot, { optional: true, self: true });
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private liveRegion?: HTMLElement;

  // --- Signal Forms -----------------------------------------------------------------------------

  protected onSignalSubmit(): void {
    const root = this.formRoot;
    if (!root) {
      return;
    }
    const state = root.fieldTree()();
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

  // --- Reactive Forms (transitional; removed at Phase 4 exit) ------------------------------------

  private readonly formDir = inject(FormGroupDirective, { optional: true, self: true });
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const formDir = this.formDir;
    if (!formDir) {
      return;
    }
    formDir.ngSubmit.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const form = formDir.form;
      if (form.invalid) {
        form.markAllAsTouched();
        this.announce(this.countInvalid(form));
        this.focusFirstInvalidElement();
      }
    });
  }

  /** Counts invalid leaf controls (descends into nested groups/arrays). */
  private countInvalid(control: AbstractControl): number {
    const children = (control as { controls?: Record<string, AbstractControl> | AbstractControl[] })
      .controls;
    if (!children) {
      return control.invalid ? 1 : 0;
    }
    const list = Array.isArray(children) ? children : Object.values(children);
    return list.reduce((sum, child) => sum + this.countInvalid(child), 0);
  }

  private focusFirstInvalidElement(): void {
    const host = this.host.nativeElement;
    const invalid = Array.from(host.querySelectorAll<HTMLElement>(".ng-invalid")).find(
      (el) => el !== host && el.tagName !== "FORM",
    );
    if (!invalid) {
      return;
    }

    invalid.scrollIntoView({ behavior: "smooth", block: "center" });

    const focusTarget = invalid.matches("input, select, textarea")
      ? invalid
      : (invalid.querySelector<HTMLElement>("input, select, textarea, [tabindex]") ?? invalid);
    focusTarget.focus?.({ preventScroll: true });
  }

  // ----------------------------------------------------------------------------------------------

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
