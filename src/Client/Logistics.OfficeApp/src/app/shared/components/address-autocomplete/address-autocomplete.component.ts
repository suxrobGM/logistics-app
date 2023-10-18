/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from '@angular/forms';


@Component({
  selector: 'app-address-autocomplete',
  standalone: true,
  templateUrl: './address-autocomplete.component.html',
  styleUrls: ['./address-autocomplete.component.scss'],
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressAutocompleteComponent,
      multi: true,
    },
  ],
})
export class AddressAutocompleteComponent implements ControlValueAccessor {
  public searchResults: GeocodingFeature[] = [];
  private isTouched = false;
  private isDisabled = false;

  @Input({required: true}) accessToken!: string;
  @Input() field = '';
  @Input() placeholder = 'Type address...';
  @Input() country = 'us';
  @Input() address: string | null = null;
  @Input() forceSelection = false;
  @Output() addressChange = new EventEmitter<string>();
  @Output() selectedAddress = new EventEmitter<SelectedAddressEvent>();

  async onAddressInputChange(event: Event) {
    if (this.isDisabled) {
      return;
    }

    const query = (event.target as HTMLInputElement)?.value;

    if (!query) {
      this.searchResults = [];
      this.markAsTouched();
      return;
    }

    const response = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${query}.json?access_token=${this.accessToken}&country=${this.country}&types=address`);
    const responseData = await response.json() as GeocodingResponse;
    this.searchResults = responseData.features;

    this.markAsTouched();
  }

  onClickAddress(address: string, center: [number, number]) {
    if (this.isDisabled) {
      return;
    }

    this.address = address;
    this.addressChange.emit(address);
    this.selectedAddress.emit({address: address, center});
    this.searchResults = [];
    this.onChange(address);
    this.markAsTouched();
  }

  onInputFocusOut(event: FocusEvent) {
    // Delay the execution to allow click event to be processed (in case an address is clicked from the list)
    setTimeout(() => {
      if (this.forceSelection && this.searchResults.length) {
        this.address = null;
        this.onChange(null);
        this.searchResults = [];
      }
    }, 100);
  }

  private onChange(value: any): void {}
  private onTouched(): void {}

  private markAsTouched() {
    if (!this.isTouched) {
      this.onTouched();
      this.isTouched = true;
    }
  }

  // #region Reactive forms methods

  writeValue(value: string): void {
    this.address = value;
  }

  registerOnChange(onChange: any): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: any): void {
    this.onTouched = onTouched;
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
}

export interface SelectedAddressEvent {
  address: string;
  center: [number, number];
}
