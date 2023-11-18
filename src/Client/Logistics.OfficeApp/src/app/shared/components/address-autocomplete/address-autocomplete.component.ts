/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from '@angular/forms';
import {Address} from '@core/models';
import {Converters} from '@shared/utils';


@Component({
  selector: 'app-address-autocomplete',
  standalone: true,
  templateUrl: './address-autocomplete.component.html',
  styleUrls: ['./address-autocomplete.component.scss'],
  imports: [
    CommonModule,
    FormsModule,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressAutocompleteComponent,
      multi: true,
    },
  ],
})
export class AddressAutocompleteComponent implements ControlValueAccessor, OnInit {
  public searchResults: GeocodingFeature[] = [];
  public addressString: string | null = null;
  private isDisabled = false;

  @Input({required: true}) accessToken!: string;
  @Input() field = '';
  @Input() placeholder = 'Type address...';
  @Input() country = 'us';
  @Input() address: Address | null = null;
  @Input() forceSelection = false;
  @Output() addressChange = new EventEmitter<Address>();
  @Output() selectedAddress = new EventEmitter<SelectedAddressEvent>();

  ngOnInit(): void {
    this.setAddressString(this.address);
  }

  async handleAddressInputChange(event: Event) {
    if (this.isDisabled) {
      return;
    }

    const query = (event.target as HTMLInputElement)?.value;

    if (!query) {
      this.searchResults = [];
      return;
    }

    const response = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${query}.json?access_token=${this.accessToken}&country=${this.country}&types=address`);
    const responseData = await response.json() as GeocodingResponse;
    this.searchResults = responseData.features;
  }

  handleClickAddress(geocodingFeature: GeocodingFeature) {
    if (this.isDisabled) {
      return;
    }

    const street = geocodingFeature.place_name.substring(0, geocodingFeature.place_name.indexOf(','));
    const city = geocodingFeature.context.find((i) => i.id.startsWith('place'))?.text ?? '';
    const region = geocodingFeature.context.find((i) => i.id.startsWith('region'))?.text ?? '';
    const zipCode = geocodingFeature.context.find((i) => i.id.startsWith('postcode'))?.text ?? '';
    const country = geocodingFeature.context.find((i) => i.id.startsWith('country'))?.text ?? '';

    const addressObj: Address = {
      line1: street,
      city: city,
      region: region,
      zipCode: zipCode,
      country: country,
    };

    this.address = addressObj;
    this.addressChange.emit(addressObj);
    this.selectedAddress.emit({
      address: addressObj,
      center: geocodingFeature.center,
    });

    this.searchResults = [];
    this.onChange(addressObj);
  }

  handleInputFocusOut(event: FocusEvent) {
    // Delay the execution to allow click event to be processed (in case an address is clicked from the list)
    setTimeout(() => {
      if (this.forceSelection && this.searchResults.length) {
        this.address = null;
        this.onChange(null);
        this.searchResults = [];
      }
    }, 100);
  }

  private setAddressString(address: Address | null) {
    this.addressString = Converters.addressToString(address);
  }

  // #region Reactive forms methods

  private onChange(value: Address | null): void {}
  private onTouched(): void {}

  writeValue(value: Address): void {
    this.address = value;
    this.setAddressString(value);
  }

  registerOnChange(fn: (value: Address | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  // #endregion
}

interface GeocodingResponse {
  type: string;
  query: string[];
  features: GeocodingFeature[];
}

interface GeocodingFeature {
  id: string;
  address: string;
  place_name: string;
  center: [number, number];
  context: FeatureContext[];
}

interface FeatureContext {
  id: string;
  text: string;
}

export interface SelectedAddressEvent {
  address: Address;
  center: [number, number];
}
