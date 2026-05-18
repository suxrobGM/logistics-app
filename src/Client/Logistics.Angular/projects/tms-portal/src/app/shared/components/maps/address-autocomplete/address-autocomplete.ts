/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-empty-function */
import { HttpClient, HttpParams } from "@angular/common/http";
import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import { FormsModule, NG_VALUE_ACCESSOR, type ControlValueAccessor } from "@angular/forms";
import type { Address } from "@logistics/shared/api";
import { regionAllowedCountries } from "@logistics/shared/utils";
import { InputTextModule } from "primeng/inputtext";
import { catchError } from "rxjs";
import { TenantService } from "@/core/services";
import { environment } from "@/env";
import type {
  GeoPoint,
  MapboxGeocodingFeature,
  MapboxGeocodingResponse,
} from "@/shared/types/mapbox";
import { Converters } from "@/shared/utils";

// Mapbox rejects more than 5 ISO codes in a single `country` filter.
const MAPBOX_MAX_COUNTRY_CODES = 5;

@Component({
  selector: "app-address-autocomplete",
  templateUrl: "./address-autocomplete.html",
  styleUrl: "./address-autocomplete.css",
  imports: [FormsModule, InputTextModule],
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
  private readonly tenantService = inject(TenantService);
  private isDisabled = false;
  private isTouched = false;
  private readonly accessToken = environment.mapboxToken;
  protected readonly searchResults = signal<MapboxGeocodingFeature[]>([]);
  protected readonly addressString = model<string | null>(null);

  public readonly field = input("");
  public readonly placeholder = input("Type address...");
  // When omitted, falls back to the tenant's region (see `effectiveCountry`).
  public readonly country = input<string | null>(null);
  public readonly forceSelection = input(false);

  private readonly effectiveCountry = computed<string>(() => {
    const override = this.country();
    if (override) {
      return override;
    }

    const region = this.tenantService.tenantData()?.settings?.region;
    return regionAllowedCountries(region)
      .slice(0, MAPBOX_MAX_COUNTRY_CODES)
      .map((c) => c.toLowerCase())
      .join(",");
  });

  private readonly effectiveLanguage = computed<string>(
    () => this.tenantService.tenantData()?.settings?.language ?? "en",
  );
  public readonly address = model<Address | null>(null);
  public readonly addressChange = output<Address>();
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

    let params = new HttpParams()
      .set("q", query)
      .set("access_token", this.accessToken)
      .set("types", "address")
      .set("language", this.effectiveLanguage());

    const countryFilter = this.effectiveCountry();
    if (countryFilter) {
      params = params.set("country", countryFilter);
    }

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

    const addressObj: Address = {
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

  private setAddressString(address: Address | null): void {
    this.addressString.set(Converters.addressToString(address));
  }

  // #region Reactive forms methods

  private onChange(value: Address | null): void {}
  private onTouched(): void {}

  private markAsTouched() {
    if (!this.isTouched) {
      this.isTouched = true;
      this.onTouched();
    }
  }

  writeValue(value: Address): void {
    this.address.set(value);
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

export interface SelectedAddressEvent {
  address: Address;
  center: GeoPoint;
}
