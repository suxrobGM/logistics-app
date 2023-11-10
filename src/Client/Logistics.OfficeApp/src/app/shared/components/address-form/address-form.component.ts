import {CommonModule} from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  forwardRef,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {ValidationSummaryComponent} from '../validation-summary/validation-summary.component';


@Component({
  selector: 'app-address-form',
  standalone: true,
  templateUrl: './address-form.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ValidationSummaryComponent,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AddressFormComponent),
      multi: true
    }
  ]
})
export class AddressFormComponent implements OnInit, OnChanges, ControlValueAccessor {
  public form: FormGroup<AddressForm>;
  private onTouched?: () => void;
  private onChanged?: (value: string | null) => void;

  @Input() addressString = '';
  @Output() addressStringChange = new EventEmitter<string | null>();

  constructor() {
    this.form = new FormGroup<AddressForm>({
      addressLine1: new FormControl('', {validators: Validators.required, nonNullable: true}),
      addressLine2: new FormControl(''),
      city: new FormControl('', {validators: Validators.required, nonNullable: true}),
      region: new FormControl('', {validators: Validators.required, nonNullable: true}),
      zipCode: new FormControl('', {validators: Validators.required, nonNullable: true}),
      country: new FormControl('', {validators: Validators.required, nonNullable: true})
    });
  }

  ngOnInit() {
    this.form.valueChanges.subscribe((values) => {
      // const address = Object.values(values).filter(Boolean).join(', ');
      const address = this.convertToString(values);
      
      if (this.onChanged) {
        this.onChanged(address);
        this.addressStringChange.emit(address);
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['addressString']) {
      this.parseAddressString(changes['addressString'].currentValue);
    }
  }

  writeValue(value: never): void {
    if (value) {
      this.form.setValue(value, {emitEvent: false});
    }
  }

  registerOnChange(fn: never): void {
    this.onChanged = fn;
  }

  registerOnTouched(fn: never): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    isDisabled ? this.form.disable() : this.form.enable();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private convertToString(values: any): string | null {
    const requiredFieldsFilled = this.form.get('addressLine1')?.value &&
      this.form.get('city')?.value &&
      this.form.get('region')?.value &&
      this.form.get('zipCode')?.value &&
      this.form.get('country')?.value;

    // If all required fields are filled, construct the address string.
    if (requiredFieldsFilled) {
      const {addressLine2, ...otherValues} = values;
      let address = Object.values(otherValues).filter(Boolean).join(', ');
      address += addressLine2 ? `, ${addressLine2}` : '';
      return address;
    }

    return null;
  }

  private parseAddressString(address: string): void {
    if (!address) {
      return;
    }

    const addressRegex = /^(.*?)(?:, (.*?))?(?:, (.*?))?, ([^,]+), (\d+), (.*)$/;
    const match = address.match(addressRegex);
    
    if (match) {
      this.form.setValue({
        addressLine1: match[1] ?? '',
        addressLine2: match[2] ?? '',
        city: match[3] ?? '',
        region: match[4] ?? '',
        zipCode: match[5] ?? '',
        country: match[6] ?? ''
      },
      {emitEvent: false});
    }
  }
}

interface AddressForm {
  addressLine1: FormControl<string>,
  addressLine2: FormControl<string | null>,
  city: FormControl<string>,
  region: FormControl<string>,
  zipCode: FormControl<string>,
  country: FormControl<string>,
}
