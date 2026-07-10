import {
  booleanAttribute,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  input,
  model,
  output,
  signal,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { InputGroupModule } from "primeng/inputgroup";
import { InputMaskModule } from "primeng/inputmask";
import { SelectModule } from "primeng/select";
import { DEFAULT_PHONE_COUNTRY, PHONE_COUNTRIES, type PhoneCountry } from "../../../constants";
import { focusFirstControl } from "../../../forms/focus-control";

/**
 * Phone number input with a country dial-code selector.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The public `value` is an E.164 string (`+[dialCode][digits]`). It is split into the
 * `selectedCountry` + `phoneNumber` presentation signals when set from OUTSIDE, and
 * recomposed from them on user edits. The recompose is idempotent (parsing what we just
 * emitted yields the same country/number), and an internal guard prevents the
 * emit -> parse round trip from clobbering local state — see `lastEmitted`.
 */
@Component({
  selector: "ui-phone-field",
  templateUrl: "./phone-field.html",
  imports: [InputGroupModule, InputMaskModule, SelectModule, FormsModule],
})
export class PhoneField implements FormValueControl<string | null> {
  /** The control's value in E.164 format. Required by `FormValueControl`. */
  public readonly value = model<string | null>(null);

  // Optional state inputs. Signal Forms binds these automatically when present;
  // the Reactive Forms bridge drives `disabled`.
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  public readonly placeholder = input<string>("Enter phone number");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected readonly countries = PHONE_COUNTRIES;
  protected readonly selectedCountry = signal<PhoneCountry>(DEFAULT_PHONE_COUNTRY);
  protected readonly phoneNumber = signal<string>("");

  protected readonly currentMask = computed(() => this.selectedCountry().mask);

  /**
   * The last value we emitted from a user edit. When `value()` matches it, the change
   * originated here, so the parse effect skips re-deriving country/number — this keeps a
   * user-selected country even when the composed value is `null` (empty number).
   */
  private lastEmitted: string | null | undefined = undefined;

  constructor() {
    effect(() => {
      const value = this.value();
      if (value === this.lastEmitted) {
        return;
      }
      this.lastEmitted = value;
      this.applyExternalValue(value);
    });
  }

  /** Split an incoming E.164 value into the country + number presentation signals. */
  private applyExternalValue(value: string | null): void {
    if (!value) {
      this.selectedCountry.set(DEFAULT_PHONE_COUNTRY);
      this.phoneNumber.set("");
      return;
    }

    // Parse E.164 format: +1234567890
    const parsed = this.parsePhoneNumber(value);
    if (parsed) {
      this.selectedCountry.set(parsed.country);
      this.phoneNumber.set(parsed.number);
    } else {
      // If we can't parse, just use the raw value
      this.phoneNumber.set(value);
    }
  }

  onCountryChange(country: PhoneCountry): void {
    this.selectedCountry.set(country);
    this.emitValue();
  }

  onPhoneChange(value: string): void {
    this.phoneNumber.set(value);
    this.emitValue();
  }

  onBlur(): void {
    this.touch.emit();
  }

  private emitValue(): void {
    const phone = this.phoneNumber();
    if (!phone) {
      this.setValue(null);
      return;
    }

    // Strip non-digit characters from the phone number
    const digitsOnly = phone.replace(/\D/g, "");
    if (!digitsOnly) {
      this.setValue(null);
      return;
    }

    // Emit in E.164 format: +[country code][number]
    this.setValue(`${this.selectedCountry().dialCode}${digitsOnly}`);
  }

  private setValue(composed: string | null): void {
    // Record before setting so the parse effect recognises this as a self-originated
    // change and leaves the local country/number signals untouched.
    this.lastEmitted = composed;
    this.value.set(composed);
  }

  private parsePhoneNumber(value: string): { country: PhoneCountry; number: string } | null {
    if (!value.startsWith("+")) {
      return null;
    }

    // Try to find matching country by dial code (longest match first)
    const sortedCountries = [...PHONE_COUNTRIES].sort(
      (a, b) => b.dialCode.length - a.dialCode.length,
    );

    for (const country of sortedCountries) {
      if (value.startsWith(country.dialCode)) {
        const number = value.slice(country.dialCode.length);
        return { country, number };
      }
    }

    return null;
  }
}
