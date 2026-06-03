import { Component, computed, contentChild, input } from "@angular/core";
import { toObservable, toSignal } from "@angular/core/rxjs-interop";
import { AbstractControl, NgControl } from "@angular/forms";
import { of, switchMap } from "rxjs";

@Component({
  selector: "ui-form-field",
  templateUrl: "./form-field.html",
})
export class FormField {
  public readonly label = input<string | null>(null);
  public readonly for = input<string | null>(null);
  public readonly required = input(false);
  public readonly hint = input<string | null>(null);

  /**
   * Optional explicit control. When omitted, the field auto-resolves the control from the
   * projected `formControlName` / `formControl` directive, so inline validation errors render
   * without any per-field wiring.
   */
  public readonly control = input<AbstractControl | null>(null);
  private readonly projected = contentChild(NgControl, { descendants: true });

  protected readonly resolvedControl = computed<AbstractControl | null>(
    () => this.control() ?? this.projected()?.control ?? null,
  );

  // Re-emits on every value/status/touched/pristine change of the resolved control so the
  // invalid/error state below stays reactive (control state is not signal-based in reactive forms).
  private readonly controlState = toSignal(
    toObservable(this.resolvedControl).pipe(switchMap((c) => c?.events ?? of(null))),
  );

  protected readonly isFieldInvalid = computed(() => {
    this.controlState(); // reactive trigger
    const control = this.resolvedControl();
    return !!control && control.invalid && (control.touched || control.dirty);
  });

  protected readonly errors = computed(() => {
    this.controlState(); // reactive trigger
    return this.resolvedControl()?.errors ?? null;
  });
}
