import { Component, computed, inject, input, output, signal } from "@angular/core";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import type { LngLatBoundsLike, LngLatLike, Map } from "mapbox-gl";
import { MapComponent, MarkerComponent, PopupComponent } from "ngx-mapbox-gl";
import { MapStyleService } from "@/core/services";
import { TenantService } from "@/core/services/tenant.service";
import { MapResizeDirective } from "@/shared/directives";
import { MapContainer } from "../map-container/map-container";
import { MapControls } from "../map-controls/map-controls";

@Component({
  selector: "app-geolocation-map",
  templateUrl: "./geolocation-map.html",
  imports: [
    AddressPipe,
    MapComponent,
    MarkerComponent,
    PopupComponent,
    MapContainer,
    MapControls,
    MapResizeDirective,
  ],
})
export class GeolocationMap {
  private readonly mapStyleService = inject(MapStyleService);
  private readonly tenantService = inject(TenantService);

  /** Region-aware default center: US → continental US; EU → central Europe. */
  private readonly defaultCenter = computed<LngLatLike>(() =>
    this.tenantService.tenantData()?.settings?.region === "eu" ? [10, 50] : [-95, 35],
  );
  private readonly defaultZoom = computed<number>(() =>
    this.tenantService.tenantData()?.settings?.region === "eu" ? 4 : 3,
  );

  protected readonly selectedMarker = signal<Marker | null>(null);
  protected readonly mapInstance = signal<Map | null>(null);

  /** Mapbox style URL based on current layer and theme */
  protected readonly mapStyle = this.mapStyleService.currentStyle;

  /** Whether there's no geolocation data */
  protected readonly isEmpty = computed(() => {
    const data = this.geolocationData();
    return !data || data.length === 0 || !data.some((d) => d.currentLocation);
  });

  /** Calculate bounds to fit all markers */
  protected readonly bounds = computed<LngLatBoundsLike | null>(() => {
    const data = this.geolocationData().filter((d) => d.currentLocation);
    if (data.length === 0) return null;

    const lngs = data.map((d) => d.currentLocation!.longitude ?? 0);
    const lats = data.map((d) => d.currentLocation!.latitude ?? 0);

    return [
      [Math.min(...lngs), Math.min(...lats)],
      [Math.max(...lngs), Math.max(...lats)],
    ];
  });

  public readonly geolocationData = input<TruckGeolocationDto[]>([]);
  /** The zoom level. Region-aware default (3 for US, 4 for EU). */
  public readonly zoom = input<number>(this.defaultZoom());
  /** The center coordinates. Region-aware default ([-95, 35] for US, [10, 50] for EU). */
  public readonly center = input<LngLatLike>(this.defaultCenter());
  public readonly width = input("100%");
  public readonly height = input("100%");

  /** Show map controls */
  public readonly showControls = input(true);

  /** Show layer toggle in controls */
  public readonly showLayerToggle = input(true);

  /** Whether the map is in a loading state (controlled by parent) */
  public readonly loading = input(false);

  /** Emitted when a truck marker is selected */
  public readonly truckSelect = output<TruckGeolocationDto>();

  /** Public method to fit map to all markers */
  public fitToBounds(): void {
    // Trigger reactivity by touching bounds
    // The template already uses this.bounds() for fitBounds
  }

  protected onSelectMarker(geoData: TruckGeolocationDto, markerComponent: MarkerComponent): void {
    this.selectedMarker.set({
      component: markerComponent,
      data: geoData,
    });
    this.truckSelect.emit(geoData);
  }

  protected onMarkerKeydown(
    event: KeyboardEvent,
    geoData: TruckGeolocationDto,
    markerComponent: MarkerComponent,
  ): void {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      this.onSelectMarker(geoData, markerComponent);
    }
  }

  protected closePopup(): void {
    this.selectedMarker.set(null);
  }

  protected onMapLoad(map: Map): void {
    this.mapInstance.set(map);
  }
}

interface Marker {
  component: MarkerComponent;
  data: TruckGeolocationDto;
}
