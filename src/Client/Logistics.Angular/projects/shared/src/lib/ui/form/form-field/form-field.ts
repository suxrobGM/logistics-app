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

/** Reads an error's validator-specific payload (e.g. `minLength`). */
function payloadOf(error: FieldError): Record<string, unknown> {
  return error as unknown as Record<string, unknown>;
}

/**
 * Fallback copy for errors that carry no `message`.
 *
 * Prefer `{message: '...'}` on the validator itself; this only covers rules declared without one.
 * Note Signal Forms' built-in error `kind`s are camelCase (`minLength`) and their payload prop is
 * `minLength`, not reactive forms' `requiredLength`.
 *
 * @see signal-forms-v22-api-probe.spec.ts, claim J
 */
function describeError(error: FieldError): string {
  if (typeof error.message === "string" && error.message.length > 0) {
    return error.message;
  }

  const payload = payloadOf(error);
  const num = (...keys: string[]): number | undefined => {
    for (const key of keys) {
      const value = payload[key];
      if (typeof value === "number") {
        return value;
      }
    }
    return undefined;
  };

  switch (error.kind) {
    case "required":
      return "This field is required.";
    case "email":
      return "Enter a valid email address.";
    case "minLength": {
      const n = num("minLength");
      return n === undefined ? "Value is too short." : `Minimum length is ${n} characters.`;
    }
    case "maxLength": {
      const n = num("maxLength");
      return n === undefined ? "Value is too long." : `Maximum length is ${n} characters.`;
    }
    case "min":
    case "minDate": {
      const n = num("min", "minDate");
      return n === undefined ? "Value is too small." : `Must be at least ${n}.`;
    }
    case "max":
    case "maxDate": {
      const n = num("max", "maxDate");
      return n === undefined ? "Value is too large." : `Must be at most ${n}.`;
    }
    case "pattern":
      return "Invalid format.";
    default:
      return "Invalid value.";
  }
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
