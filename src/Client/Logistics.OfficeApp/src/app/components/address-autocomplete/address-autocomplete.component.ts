/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import {CommonModule} from "@angular/common";
import {Component, OnInit, input, model, output, signal} from "@angular/core";
import {ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR} from "@angular/forms";
import {AddressDto} from "@/core/api/models";
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
  private isDisabled = false;
  private isTouched = false;
  public readonly searchResults = signal<GeocodingFeature[]>([]);
  public readonly addressString = model<string | null>(null);

  public readonly accessToken = input.required<string>();
  public readonly field = input("");
  public readonly placeholder = input("Type address...");
  public readonly country = input("us");
  public readonly forceSelection = input(false);
  public readonly address = model<AddressDto | null>(null);
  public readonly addressChange = output<AddressDto>();
  public readonly selectedAddress = output<SelectedAddressEvent>();

  ngOnInit(): void {
    this.setAddressString(this.address());
  }

  async handleAddressInputChange(event: Event): Promise<void> {
    if (this.isDisabled) {
      return;
    }

    const query = (event.target as HTMLInputElement)?.value;

    if (!query) {
      this.markAsTouched();
      this.searchResults.set([]);
      return;
    }

    const response = await fetch(
      `https://api.mapbox.com/search/geocode/v6/forward?q=${query}&access_token=${this.accessToken()}&country=${this.country()}&types=address`
    );

    const responseData = (await response.json()) as GeocodingResponse;
    this.searchResults.set(responseData.features);
  }

  handleClickAddress(geocodingFeature: GeocodingFeature): void {
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

    this.address.set(addressObj);
    this.addressChange.emit(addressObj);
    this.selectedAddress.emit({
      address: addressObj,
      center: geocodingFeature.center,
    });

    this.searchResults.set([]);
    this.setAddressString(addressObj);
    this.onChange(addressObj);
    this.markAsTouched();
  }

  handleInputFocusOut(event: FocusEvent): void {
    // Delay the execution to allow click event to be processed (in case an address is clicked from the list)
    setTimeout(() => {
      if (this.forceSelection() && this.searchResults.length) {
        this.address.set(null);
        this.addressString.set(null);
        this.searchResults.set([]);
        this.onChange(null);
      }
    }, 100);
  }

  private setAddressString(address: AddressDto | null): void {
    this.addressString.set(Converters.addressToString(address));
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
    this.address.set(value);
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
