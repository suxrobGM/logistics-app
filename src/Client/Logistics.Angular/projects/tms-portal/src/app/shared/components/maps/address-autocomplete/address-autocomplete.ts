import { HttpClient, HttpParams } from "@angular/common/http";
import {
  Component,
  computed,
  ElementRef,
  inject,
  input,
  linkedSignal,
  model,
  output,
  signal,
} from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import type { Address } from "@logistics/shared/api";
import { UiTextField } from "@logistics/shared/ui";
import { regionAllowedCountries } from "@logistics/shared/utils";
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
  imports: [UiTextField],
})
export class AddressAutocomplete implements FormValueControl<Address | null> {
  private readonly http = inject(HttpClient);
  private readonly tenantService = inject(TenantService);
  private isTouched = false;
  private readonly accessToken = environment.mapboxToken;
  protected readonly searchResults = signal<MapboxGeocodingFeature[]>([]);

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<Address | null>(null);
  /** Driven by the Reactive Forms bridge; gates all user interaction. */
  public readonly disabled = input<boolean>(false);
  /** Raised the first time the field is interacted with so the form marks it touched. */
  public readonly touch = output<void>();

  /**
   * Text rendered in the input. Reseeds from `value` whenever the form pushes a new
   * address (the old `writeValue` behaviour); the user's typing overrides it until the
   * next `value` change. `addressString` never writes back to `value`, so there is no
   * value -> parse -> value loop.
   */
  protected readonly addressString = linkedSignal<string | null>(() =>
    Converters.addressToString(this.value()),
  );

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
  public readonly selectedAddress = output<SelectedAddressEvent>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected handleAddressInputChange(query: string): void {
    if (this.disabled()) {
      return;
    }

    // `ui-text-field` is a `FormValueControl<string>`, so it hands us a plain string. Mirror it
    // into `addressString` (the linkedSignal that renders the input) before searching.
    this.addressString.set(query);

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
    if (this.disabled()) {
      return;
    }

    const street = geocodingFeature.properties.context.address.name;
    const city = geocodingFeature.properties.context.place.name;
    const region = geocodingFeature.properties.context.region.name;
    const zipCode = geocodingFeature.properties.context.postcode.name;
    // Use the ISO-3166-1 alpha-2 code (Mapbox returns it lowercase, e.g. "us"),
    // uppercased to match the rest of the app. The backend requires a 2-letter code.
    const country = geocodingFeature.properties.context.country.country_code?.toUpperCase();

    const addressObj: Address = {
      line1: street,
      city: city,
      state: region,
      zipCode: zipCode,
      country: country,
    };

    // Reseeds `addressString` via the linkedSignal above.
    this.value.set(addressObj);
    this.selectedAddress.emit({
      address: addressObj,
      center: geocodingFeature.geometry.coordinates,
    });

    this.searchResults.set([]);
    this.markAsTouched();
  }

  protected handleInputFocusOut(): void {
    // Delay the execution to allow click event to be processed (in case an address is clicked from the list)
    setTimeout(() => {
      if (this.forceSelection() && this.searchResults.length) {
        this.value.set(null);
        this.addressString.set(null);
        this.searchResults.set([]);
      }
    }, 100);
  }

  private markAsTouched(): void {
    if (!this.isTouched) {
      this.isTouched = true;
      this.touch.emit();
    }
  }
}

export interface SelectedAddressEvent {
  address: Address;
  center: GeoPoint;
}
