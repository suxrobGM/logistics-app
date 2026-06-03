import { DestroyRef, Directive, ElementRef, inject, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { AbstractControl, FormGroupDirective } from "@angular/forms";

/**
 * Reveals validation feedback when an invalid reactive form is submitted.
 *
 * Auto-applies to every `<form [formGroup]>` in a component that imports it — no template
 * attribute and no submit-handler changes are required. On an invalid submit it:
 *  - marks every control as touched (so inline `ui-form-field` errors render), and
 *  - scrolls to / focuses the first invalid control, and
 *  - announces the error count to assistive technologies via an `aria-live` region.
 *
 * This is the single, reusable replacement for hand-written `markAllAsTouched()` calls.
 */
@Directive({
  // Intentionally matches every reactive form so the behavior is opt-in by import only.
  // eslint-disable-next-line @angular-eslint/directive-selector
  selector: "form[formGroup]",
})
export class ValidatedForm implements OnInit {
  private readonly formDir = inject(FormGroupDirective);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);
  private liveRegion?: HTMLElement;

  ngOnInit(): void {
    this.formDir.ngSubmit.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const form = this.formDir.form;
      if (form.invalid) {
        form.markAllAsTouched();
        this.revealErrors();
      }
    });
  }

  private revealErrors(): void {
    const count = this.countInvalid(this.formDir.form);
    this.announce(
      count === 1
        ? "1 field needs your attention before submitting."
        : `${count} fields need your attention before submitting.`,
    );
    this.focusFirstInvalid();
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

  private focusFirstInvalid(): void {
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

  private announce(message: string): void {
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
