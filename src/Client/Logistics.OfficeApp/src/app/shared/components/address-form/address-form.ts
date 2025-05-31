import {CommonModule} from "@angular/common";
import {Component, input, output} from "@angular/core";
import {
  ControlValueAccessor,
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";
import {KeyFilterModule} from "primeng/keyfilter";
import {SelectModule} from "primeng/select";
import {AddressDto} from "@/core/api/models";
import {COUNTRIES_OPTIONS, DEFAULT_COUNTRY_OPTION, US_STATES_OPTIONS} from "@/core/constants";
import {findOption} from "@/shared/utils";
import {ValidationSummary} from "../validation-summary/validation-summary";

@Component({
  selector: "app-address-form",
  standalone: true,
  templateUrl: "./address-form.html",
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ValidationSummary,
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
  readonly form: FormGroup<AddressFormType>;
  readonly usStates = US_STATES_OPTIONS;
  readonly countries = COUNTRIES_OPTIONS;
  private onTouched?: () => void;
  private onChanged?: (value: AddressDto | null) => void;

  readonly address = input<AddressDto>();
  readonly addressChange = output<AddressDto | null>();

  constructor() {
    this.form = new FormGroup<AddressFormType>({
      addressLine1: new FormControl("", {validators: Validators.required, nonNullable: true}),
      addressLine2: new FormControl(null),
      city: new FormControl("", {validators: Validators.required, nonNullable: true}),
      state: new FormControl("", {validators: Validators.required, nonNullable: true}),
      zipCode: new FormControl("", {validators: Validators.required, nonNullable: true}),
      country: new FormControl(
        {value: DEFAULT_COUNTRY_OPTION.value, disabled: true},
        {validators: Validators.required, nonNullable: true}
      ),
    });

    this.form.valueChanges.subscribe((values) => this.handleFormValueChange(values));
  }

  writeValue(value: AddressDto | null): void {
    if (!value) {
      return;
    }

    const usStateOption = findOption(US_STATES_OPTIONS, value.state);
    const countryOption = findOption(COUNTRIES_OPTIONS, value.country);

    this.form.setValue(
      {
        addressLine1: value.line1,
        addressLine2: value.line2 ?? null,
        city: value.city,
        state: usStateOption?.value ?? "",
        zipCode: value.zipCode,
        country: countryOption?.value ?? DEFAULT_COUNTRY_OPTION.value,
      },
      {emitEvent: false}
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
      this.form.disable({emitEvent: false});
    }
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private handleFormValueChange(values: any): void {
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

    const address: AddressDto = {
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
