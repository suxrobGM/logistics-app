import { Component, computed, input, signal } from "@angular/core";
import { type ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { InputGroupModule } from "primeng/inputgroup";
import { InputMaskModule } from "primeng/inputmask";
import { SelectModule } from "primeng/select";
import { DEFAULT_PHONE_COUNTRY, PHONE_COUNTRIES, type PhoneCountry } from "../../../constants";

@Component({
  selector: "ui-phone-input",
  templateUrl: "./phone-input.html",
  imports: [InputGroupModule, InputMaskModule, SelectModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: PhoneInput,
      multi: true,
    },
  ],
})
export class PhoneInput implements ControlValueAccessor {
  protected readonly countries = PHONE_COUNTRIES;
  protected readonly selectedCountry = signal<PhoneCountry>(DEFAULT_PHONE_COUNTRY);
  protected readonly phoneNumber = signal<string>("");
  protected readonly disabled = signal(false);

  public readonly placeholder = input<string>("Enter phone number");

  protected readonly currentMask = computed(() => this.selectedCountry().mask);

  private onTouched?: () => void;
  private onChanged?: (value: string | null) => void;

  writeValue(value: string | null): void {
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

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChanged = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
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
    this.onTouched?.();
  }

  private emitValue(): void {
    const phone = this.phoneNumber();
    if (!phone) {
      this.onChanged?.(null);
      return;
    }

    // Strip non-digit characters from the phone number
    const digitsOnly = phone.replace(/\D/g, "");
    if (!digitsOnly) {
      this.onChanged?.(null);
      return;
    }

    // Emit in E.164 format: +[country code][number]
    const fullNumber = `${this.selectedCountry().dialCode}${digitsOnly}`;
    this.onChanged?.(fullNumber);
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
