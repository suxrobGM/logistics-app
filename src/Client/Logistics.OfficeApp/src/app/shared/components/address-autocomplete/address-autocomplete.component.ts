import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {NgFor, NgIf} from '@angular/common';
import {ControlValueAccessor, NG_VALUE_ACCESSOR} from '@angular/forms';


@Component({
  selector: 'app-address-autocomplete',
  standalone: true,
  templateUrl: './address-autocomplete.component.html',
  styleUrls: ['./address-autocomplete.component.scss'],
  imports: [NgFor, NgIf],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressAutocompleteComponent,
      multi: true,
    },
  ],
})
export class AddressAutocompleteComponent implements OnInit, ControlValueAccessor {
  public searchResults: GeocodingFeature[] = [];
  private isTouched = false;
  private isDisabled = false;

  @Input({required: true}) accessToken!: string;
  @Input() field: string = '';
  @Input() placeholder: string = 'Type address...';
  @Input() country: string = 'us';
  @Input() address: string = '';
  @Output() addressChange = new EventEmitter<string>();
  @Output() selectedAddress = new EventEmitter<SelectedAddressEvent>();

  ngOnInit(): void {}

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

  onClickAddress(address: string, center: number[]) {
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

  private onChange(value: any): void {};
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
  center: number[];
}

export interface SelectedAddressEvent {
  address: string;
  center: number[];
}
