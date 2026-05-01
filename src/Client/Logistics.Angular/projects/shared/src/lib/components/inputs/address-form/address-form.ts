import { Component, computed, input, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
  type ControlValueAccessor,
} from "@angular/forms";
import { InputTextModule } from "primeng/inputtext";
import { KeyFilterModule } from "primeng/keyfilter";
import { SelectModule } from "primeng/select";
import type { Address } from "../../../api/generated/models";
import { COUNTRIES_OPTIONS, DEFAULT_COUNTRY_OPTION, US_STATES_OPTIONS } from "../../../constants";
import type { SelectOption } from "../../../models/select-option";
import { findOption } from "../../../utils/select-utils";
import { FormField } from "../form-field/form-field";
import { ValidationSummary } from "../validation-summary/validation-summary";

/** Country-specific state/province option lists. Countries not listed get a free-text input. */
const COUNTRY_STATE_OPTIONS: Record<string, SelectOption[]> = {
  US: US_STATES_OPTIONS,
};

@Component({
  selector: "ui-address-form",
  templateUrl: "./address-form.html",
  imports: [
    ReactiveFormsModule,
    ValidationSummary,
    FormField,
    SelectModule,
    InputTextModule,
    KeyFilterModule,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressForm,
      multi: true,
    },
  ],
})
export class AddressForm implements ControlValueAccessor {
  public readonly form: FormGroup<AddressFormType> = new FormGroup<AddressFormType>({
    addressLine1: new FormControl("", { validators: Validators.required, nonNullable: true }),
    addressLine2: new FormControl(null),
    city: new FormControl("", { validators: Validators.required, nonNullable: true }),
    state: new FormControl("", { validators: Validators.required, nonNullable: true }),
    zipCode: new FormControl("", { validators: Validators.required, nonNullable: true }),
    country: new FormControl(DEFAULT_COUNTRY_OPTION.value, {
      validators: Validators.required,
      nonNullable: true,
    }),
  });

  private onTouched?: () => void;
  private onChanged?: (value: Address | null) => void;

  public readonly address = input<Address>();
  public readonly addressChange = output<Address | null>();
  /**
   * Optional list of ISO-3166-1 alpha-2 country codes to allow.
   * When omitted, all countries are shown.
   */
  public readonly allowedCountries = input<readonly string[] | null>(null);

  protected readonly countries = computed(() => {
    const allowed = this.allowedCountries();
    if (!allowed || allowed.length === 0) {
      return COUNTRIES_OPTIONS;
    }
    const set = new Set(allowed);
    return COUNTRIES_OPTIONS.filter((opt) => set.has(opt.value));
  });

  /** Currently selected country code. Updated from valueChanges and writeValue. */
  private readonly country = signal(this.form.controls.country.value);

  /** State/province options for the selected country, or null when free-text. */
  protected readonly stateOptions = computed<SelectOption[] | null>(
    () => COUNTRY_STATE_OPTIONS[this.country()] ?? null,
  );

  protected readonly hasStateOptions = computed(() => this.stateOptions() !== null);

  constructor() {
    // valueChanges only fires for user-driven changes (writeValue uses
    // emitEvent: false), so clearing here won't wipe a hydrated state value.
    this.form.controls.country.valueChanges.subscribe((newCountry) => {
      const prevHadOptions = !!COUNTRY_STATE_OPTIONS[this.country()];
      const newHasOptions = !!COUNTRY_STATE_OPTIONS[newCountry];
      this.country.set(newCountry);

      if (prevHadOptions !== newHasOptions) {
        this.form.controls.state.setValue("");
      }
    });
    this.form.valueChanges.subscribe(() => this.handleFormValueChange());
  }

  writeValue(value: Address | null): void {
    if (!value) {
      return;
    }

    const countryOption = findOption(COUNTRIES_OPTIONS, value.country ?? "");
    const countryCode = countryOption?.value ?? DEFAULT_COUNTRY_OPTION.value;
    const stateOptions = COUNTRY_STATE_OPTIONS[countryCode];
    const stateValue = stateOptions
      ? (findOption(stateOptions, value.state ?? "")?.value ?? "")
      : (value.state ?? "");

    // Sync the country signal so the state field renders the right input mode
    // (dropdown vs free-text) immediately. setValue with emitEvent: false skips
    // the valueChanges subscription that would otherwise drive this update.
    this.country.set(countryCode);

    this.form.setValue(
      {
        addressLine1: value.line1 ?? "",
        addressLine2: value.line2 ?? null,
        city: value.city ?? "",
        state: stateValue,
        zipCode: value.zipCode ?? "",
        country: countryCode,
      },
      { emitEvent: false },
    );
  }

  registerOnChange(fn: never): void {
    this.onChanged = fn;
  }

  registerOnTouched(fn: never): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.form.disable({ emitEvent: false });
    }
  }

  private handleFormValueChange(): void {
    const values = this.form.getRawValue();

    if (
      !values.addressLine1 ||
      !values.city ||
      !values.state ||
      !values.zipCode ||
      !values.country
    ) {
      return;
    }

    const countryOption = findOption(COUNTRIES_OPTIONS, values.country);

    const address: Address = {
      line1: values.addressLine1,
      line2: values.addressLine2,
      city: values.city,
      state: values.state,
      zipCode: values.zipCode,
      country: countryOption?.label ?? DEFAULT_COUNTRY_OPTION.label,
    };

    this.onChanged?.(address);
    this.addressChange?.emit(address);
  }
}

interface AddressFormType {
  addressLine1: FormControl<string>;
  addressLine2: FormControl<string | null>;
  city: FormControl<string>;
  state: FormControl<string>;
  zipCode: FormControl<string>;
  country: FormControl<string>;
}
