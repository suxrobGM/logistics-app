import {CommonModule} from "@angular/common";
import {Component, forwardRef, input, output} from "@angular/core";
import {
  ControlValueAccessor,
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {AddressDto} from "@/core/api/models";
import {ValidationSummaryComponent} from "../validation-summary/validation-summary.component";

@Component({
  selector: "app-address-form",
  standalone: true,
  templateUrl: "./address-form.component.html",
  imports: [CommonModule, ReactiveFormsModule, ValidationSummaryComponent],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AddressFormComponent),
      multi: true,
    },
  ],
})
export class AddressFormComponent implements ControlValueAccessor {
  readonly form: FormGroup<AddressForm>;
  private onTouched?: () => void;
  private onChanged?: (value: AddressDto | null) => void;

  readonly address = input<AddressDto>();
  readonly addressChange = output<AddressDto | null>();

  constructor() {
    this.form = new FormGroup<AddressForm>({
      addressLine1: new FormControl("", {validators: Validators.required, nonNullable: true}),
      addressLine2: new FormControl(null),
      city: new FormControl("", {validators: Validators.required, nonNullable: true}),
      state: new FormControl("", {validators: Validators.required, nonNullable: true}),
      zipCode: new FormControl("", {validators: Validators.required, nonNullable: true}),
      country: new FormControl("", {validators: Validators.required, nonNullable: true}),
    });

    this.form.valueChanges.subscribe((values) => {
      if (
        !values.addressLine1 ||
        !values.city ||
        !values.state ||
        !values.zipCode ||
        !values.country
      ) {
        return;
      }

      const address: AddressDto = {
        line1: values.addressLine1,
        line2: values.addressLine2,
        city: values.city,
        state: values.state,
        zipCode: values.zipCode,
        country: values.country,
      };

      if (this.onChanged) {
        this.onChanged(address);
        this.addressChange.emit(address);
      }
    });
  }

  writeValue(value: AddressDto | null): void {
    if (!value) {
      return;
    }

    this.form.setValue(
      {
        addressLine1: value.line1,
        addressLine2: value.line2 ?? null,
        city: value.city,
        state: value.state,
        zipCode: value.zipCode,
        country: value.country,
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
      this.form.disable();
    } else {
      this.form.enable();
    }
  }
}

interface AddressForm {
  addressLine1: FormControl<string>;
  addressLine2: FormControl<string | null>;
  city: FormControl<string>;
  state: FormControl<string>;
  zipCode: FormControl<string>;
  country: FormControl<string>;
}
