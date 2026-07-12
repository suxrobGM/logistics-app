import { Component, computed, contentChild, input } from "@angular/core";
import { FORM_FIELD, type ReadonlyFieldState } from "@angular/forms/signals";

/**
 * A single validation error.
 *
 * Deliberately has NO index signature — Signal Forms' `ValidationError` has none either, and adding
 * one here would make every error un-assignable. Payload props are read via `payloadOf()`.
 */
export interface FieldError {
  readonly kind: string;
  readonly message?: string;
}

/**
 * Renders an error's copy: the validator's own `message` when it supplied one, otherwise a single
 * generic fallback.
 *
 * Every validator in this codebase passes `{ message: '...' }`, so the field's message is used
 * verbatim. The generic sentence only covers the rare rule declared without one (e.g. a test
 * fixture or a third-party schema error) — there is deliberately no per-`kind` message table.
 */
function describeError(error: FieldError): string {
  const message = error.message;
  return typeof message === "string" && message.length > 0 ? message : "This field is invalid.";
}

@Component({
  selector: "ui-form-field",
  templateUrl: "./form-field.html",
})
export class UiFormField {
  public readonly label = input<string | null>(null);
  public readonly for = input<string | null>(null);
  public readonly required = input(false);
  public readonly hint = input<string | null>(null);

  /**
   * Optional explicit field. When omitted, the field auto-resolves from the projected `[formField]`
   * directive, so inline validation errors render without any per-field wiring.
   *
   * Bind it only when the control cannot carry `[formField]` itself — e.g. a control that is
   * driven by a plain `[(value)]` two-way binding because it needs a projected `<ng-template>`.
   *
   * Typed as a read-only field accessor rather than `Field<unknown>` so any `FieldTree<T>` is
   * assignable: `FieldState<T>.value` is a `WritableSignal<T>` (invariant), while
   * `ReadonlyFieldState<T>.value` is a `Signal<T>` (covariant).
   */
  public readonly field = input<(() => ReadonlyFieldState<unknown>) | null>(null);
  private readonly projectedField = contentChild(FORM_FIELD, { descendants: true });

  private readonly fieldState = computed<ReadonlyFieldState<unknown> | null>(() => {
    const explicit = this.field();
    return explicit ? explicit() : (this.projectedField()?.state() ?? null);
  });

  protected readonly isFieldInvalid = computed(() => {
    const state = this.fieldState();
    return !!state && state.invalid() && (state.touched() || state.dirty());
  });

  protected readonly errors = computed<readonly FieldError[]>(
    () => this.fieldState()?.errors() ?? [],
  );

  protected describe(error: FieldError): string {
    return describeError(error);
  }
}
