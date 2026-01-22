import { Component, input, output } from "@angular/core";
import {
  type ControlValueAccessor,
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { InputTextModule } from "primeng/inputtext";
import { KeyFilterModule } from "primeng/keyfilter";
import { SelectModule } from "primeng/select";
import type { Address } from "../../../api/generated/models";
import { COUNTRIES_OPTIONS, DEFAULT_COUNTRY_OPTION, US_STATES_OPTIONS } from "../../../constants";
import { findOption } from "../../../utils/select-utils";
import { ValidationSummary } from "../../form/validation-summary/validation-summary";

@Component({
  selector: "ui-address-form",
  templateUrl: "./address-form.html",
  imports: [ReactiveFormsModule, ValidationSummary, SelectModule, InputTextModule, KeyFilterModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressForm,
      multi: true,
    },
  ],
})
export class AddressForm implements ControlValueAccessor {
  readonly form: FormGroup<AddressFormType>;
  readonly usStates = US_STATES_OPTIONS;
  readonly countries = COUNTRIES_OPTIONS;
  private onTouched?: () => void;
  private onChanged?: (value: Address | null) => void;

  readonly address = input<Address>();
  readonly addressChange = output<Address | null>();

  constructor() {
    this.form = new FormGroup<AddressFormType>({
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

    this.form.valueChanges.subscribe(() => this.handleFormValueChange());
  }

  writeValue(value: Address | null): void {
    if (!value) {
      return;
    }

    const usStateOption = findOption(US_STATES_OPTIONS, value.state ?? "");
    const countryOption = findOption(COUNTRIES_OPTIONS, value.country ?? "");

    this.form.setValue(
      {
        addressLine1: value.line1 ?? "",
        addressLine2: value.line2 ?? null,
        city: value.city ?? "",
        state: usStateOption?.value ?? "",
        zipCode: value.zipCode ?? "",
        country: countryOption?.value ?? DEFAULT_COUNTRY_OPTION.value,
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
