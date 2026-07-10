import { Component, computed, contentChild, input } from "@angular/core";
import { toObservable, toSignal } from "@angular/core/rxjs-interop";
import { AbstractControl, NgControl } from "@angular/forms";
import { FORM_FIELD, type Field } from "@angular/forms/signals";
import { of, switchMap } from "rxjs";

/**
 * A single validation error, normalized across Signal Forms and legacy Reactive Forms.
 *
 * Signal Forms already speaks this shape (`ValidationError`). Reactive Forms' keyed
 * `ValidationErrors` object is flattened into it by `legacyErrors()` below.
 *
 * Deliberately has NO index signature — `ValidationError` has none either, and adding one here
 * would make every Signal Forms error un-assignable. Payload props are read via `payloadOf()`.
 */
export interface FieldError {
  readonly kind: string;
  readonly message?: string;
}

/** Reads an error's validator-specific payload (e.g. `minLength`, `requiredLength`). */
function payloadOf(error: FieldError): Record<string, unknown> {
  return error as unknown as Record<string, unknown>;
}

/**
 * Fallback copy for errors that carry no `message`.
 *
 * Signal Forms' built-in error `kind`s are camelCase (`minLength`), while Reactive Forms uses
 * lowercase keys (`minlength`) and a different payload prop (`requiredLength` vs `minLength`).
 * Both spellings are handled so this component works during the reactive -> signal migration.
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
    case "minLength":
    case "minlength": {
      const n = num("minLength", "requiredLength");
      return n === undefined ? "Value is too short." : `Minimum length is ${n} characters.`;
    }
    case "maxLength":
    case "maxlength": {
      const n = num("maxLength", "requiredLength");
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

/** Flattens Reactive Forms' `{ [key]: payload }` errors into the normalized array shape. */
function legacyErrors(control: AbstractControl | null): readonly FieldError[] {
  const errors = control?.errors;
  if (!errors) {
    return [];
  }
  return Object.entries(errors).map(
    ([kind, payload]) =>
      (payload !== null && typeof payload === "object"
        ? { ...(payload as object), kind }
        : { kind, value: payload }) as FieldError,
  );
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
   * Optional explicit field. When omitted, the field auto-resolves from the projected
   * `[formField]` directive, so inline validation errors render without any per-field wiring.
   */
  public readonly field = input<Field<unknown> | null>(null);
  private readonly projectedField = contentChild(FORM_FIELD, { descendants: true });

  private readonly fieldState = computed(() => {
    const explicit = this.field();
    if (explicit) {
      return explicit();
    }
    // A projected `[formField]` also provides `NgControl` (as `InteropNgControl`), so this must be
    // checked BEFORE the legacy branch below — it is the richer of the two (ordered errors that
    // carry their own `message`).
    return this.projectedField()?.state() ?? null;
  });

  // ---------------------------------------------------------------------------------------------
  // Legacy Reactive Forms support. Deleted once the last `formControlName` is gone (Phase 4 exit).
  // ---------------------------------------------------------------------------------------------

  /** @deprecated Reactive-forms only. Bind nothing; `[formField]` auto-resolves. */
  public readonly control = input<AbstractControl | null>(null);
  private readonly projectedControl = contentChild(NgControl, { descendants: true });

  private readonly legacyControl = computed<AbstractControl | null>(() =>
    this.fieldState() ? null : (this.control() ?? this.projectedControl()?.control ?? null),
  );

  // Reactive-forms control state is not signal-based, so re-emit on every value/status/touched
  // change to keep the computeds below live. (`InteropNgControl` has no `events`, hence `of(null)`.)
  private readonly legacyState = toSignal(
    toObservable(this.legacyControl).pipe(switchMap((c) => c?.events ?? of(null))),
  );

  // ---------------------------------------------------------------------------------------------

  protected readonly isFieldInvalid = computed(() => {
    const state = this.fieldState();
    if (state) {
      return state.invalid() && (state.touched() || state.dirty());
    }
    this.legacyState(); // reactive trigger
    const control = this.legacyControl();
    return !!control && control.invalid && (control.touched || control.dirty);
  });

  protected readonly errors = computed<readonly FieldError[]>(() => {
    const state = this.fieldState();
    if (state) {
      return state.errors();
    }
    this.legacyState(); // reactive trigger
    return legacyErrors(this.legacyControl());
  });

  protected describe(error: FieldError): string {
    return describeError(error);
  }
}
