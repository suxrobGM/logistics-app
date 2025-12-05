/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import { HttpClient, HttpParams } from "@angular/common/http";
import { Component, inject, input, model, output, signal } from "@angular/core";
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from "@angular/forms";
import { catchError } from "rxjs";
import { AddressDto } from "@/core/api/models";
import { environment } from "@/env";
import { GeoPoint, MapboxGeocodingFeature, MapboxGeocodingResponse } from "@/shared/types/mapbox";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-address-autocomplete",
  templateUrl: "./address-autocomplete.html",
  styleUrl: "./address-autocomplete.css",
  imports: [FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: AddressAutocomplete,
      multi: true,
    },
  ],
})
export class AddressAutocomplete implements ControlValueAccessor {
  private readonly http = inject(HttpClient);
  private isDisabled = false;
  private isTouched = false;
  private readonly accessToken = environment.mapboxToken;
  protected readonly searchResults = signal<MapboxGeocodingFeature[]>([]);
  protected readonly addressString = model<string | null>(null);

  public readonly field = input("");
  public readonly placeholder = input("Type address...");
  public readonly country = input("us");
  public readonly forceSelection = input(false);
  public readonly address = model<AddressDto | null>(null);
  public readonly addressChange = output<AddressDto>();
  public readonly selectedAddress = output<SelectedAddressEvent>();

  constructor() {
    this.setAddressString(this.address());
  }

  protected handleAddressInputChange(event: Event): void {
    if (this.isDisabled) {
      return;
    }

    const query = (event.target as HTMLInputElement)?.value;

    if (!query) {
      this.markAsTouched();
      this.searchResults.set([]);
      return;
    }

    const params = new HttpParams()
      .set("q", query)
      .set("access_token", this.accessToken)
      .set("country", this.country())
      .set("types", "address");

    this.http
      .get<MapboxGeocodingResponse>("https://api.mapbox.com/search/geocode/v6/forward", { params })
      .pipe(
        catchError(() => {
          this.searchResults.set([]);
          return [];
        }),
      )
      .subscribe((data) => {
        this.searchResults.set(data.features || []);
      });
  }

  protected handleClickAddress(geocodingFeature: MapboxGeocodingFeature): void {
    if (this.isDisabled) {
      return;
    }

    const street = geocodingFeature.properties.context.address.name;
    const city = geocodingFeature.properties.context.place.name;
    const region = geocodingFeature.properties.context.region.name;
    const zipCode = geocodingFeature.properties.context.postcode.name;
    const country = geocodingFeature.properties.context.country.name;

    const addressObj: AddressDto = {
      line1: street,
      city: city,
      state: region,
      zipCode: zipCode,
      country: country,
    };

    this.address.set(addressObj);
    this.addressChange.emit(addressObj);
    this.selectedAddress.emit({
      address: addressObj,
      center: geocodingFeature.geometry.coordinates,
    });

    this.searchResults.set([]);
    this.setAddressString(addressObj);
    this.onChange(addressObj);
    this.markAsTouched();
  }

  protected handleInputFocusOut(event: FocusEvent): void {
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

export interface SelectedAddressEvent {
  address: AddressDto;
  center: GeoPoint;
}
