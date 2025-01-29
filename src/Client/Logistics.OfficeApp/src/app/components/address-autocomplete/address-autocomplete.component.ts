/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import {Component, EventEmitter, Input, OnInit, Output} from "@angular/core";
import {CommonModule} from "@angular/common";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AddressDto} from "@/core/models";
import {Converters} from "@/core/utils";

@Component({
  selector: "app-address-autocomplete",
  standalone: true,
  templateUrl: "./address-autocomplete.component.html",
  styleUrls: ["./address-autocomplete.component.scss"],
  imports: [CommonModule, FormsModule],
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
  private isTouched = false;

  @Input({required: true}) accessToken!: string;
  @Input() field = "";
  @Input() placeholder = "Type address...";
  @Input() country = "us";
  @Input() forceSelection = false;
  @Input() address: AddressDto | null = null;
  @Output() addressChange = new EventEmitter<AddressDto>();
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
      this.markAsTouched();
      this.searchResults = [];
      return;
    }

    const response = await fetch(
      `https://api.mapbox.com/geocoding/v5/mapbox.places/${query}.json?access_token=${this.accessToken}&country=${this.country}&types=address`
    );
    const responseData = (await response.json()) as GeocodingResponse;
    this.searchResults = responseData.features;
  }

  handleClickAddress(geocodingFeature: GeocodingFeature) {
    if (this.isDisabled) {
      return;
    }

    const street = geocodingFeature.place_name.substring(
      0,
      geocodingFeature.place_name.indexOf(",")
    );
    const city = geocodingFeature.context.find((i) => i.id.startsWith("place"))?.text ?? "";
    const region = geocodingFeature.context.find((i) => i.id.startsWith("region"))?.text ?? "";
    const zipCode = geocodingFeature.context.find((i) => i.id.startsWith("postcode"))?.text ?? "";
    const country = geocodingFeature.context.find((i) => i.id.startsWith("country"))?.text ?? "";

    const addressObj: AddressDto = {
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
    this.setAddressString(addressObj);
    this.onChange(addressObj);
    this.markAsTouched();
  }

  handleInputFocusOut(event: FocusEvent) {
    // Delay the execution to allow click event to be processed (in case an address is clicked from the list)
    setTimeout(() => {
      if (this.forceSelection && this.searchResults.length) {
        this.address = null;
        this.addressString = null;
        this.searchResults = [];
        this.onChange(null);
      }
    }, 100);
  }

  private setAddressString(address: AddressDto | null) {
    this.addressString = Converters.addressToString(address);
  }

  // #region Reactive forms methods

  private onChange(value: AddressDto | null): void {}
  private onTouched(): void {}

  private markAsTouched() {
    if (!this.isTouched) {
      this.isTouched = true;
      this.onTouched();
    }
  }

  writeValue(value: AddressDto): void {
    this.address = value;
    this.setAddressString(value);
  }

  registerOnChange(fn: (value: AddressDto | null) => void): void {
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
  address: AddressDto;
  center: [number, number];
}
