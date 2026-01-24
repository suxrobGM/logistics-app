import { Component, computed, inject, input, signal } from "@angular/core";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { MapComponent, MarkerComponent, PopupComponent } from "ngx-mapbox-gl";
import { ThemeService } from "@/core/services";

@Component({
  selector: "app-geolocation-map",
  templateUrl: "./geolocation-map.html",
  imports: [AddressPipe, MapComponent, MarkerComponent, PopupComponent],
})
export class GeolocationMap {
  private readonly themeService = inject(ThemeService);

  protected readonly selectedMarker = signal<Marker | null>(null);

  /** Mapbox style URL based on current theme */
  protected readonly mapStyle = computed(() =>
    this.themeService.isDark()
      ? "mapbox://styles/mapbox/dark-v11"
      : "mapbox://styles/mapbox/streets-v12",
  );

  public readonly geolocationData = input<TruckGeolocationDto[]>([]);
  public readonly zoom = input<number>(3);
  public readonly center = input<[number, number]>([-95, 35]);
  public readonly width = input("100%");
  public readonly height = input("100%");

  protected onSelectMarker(geoData: TruckGeolocationDto, markerComponent: MarkerComponent): void {
    this.selectedMarker.set({
      component: markerComponent,
      data: geoData,
    });
  }
}

interface Marker {
  component: MarkerComponent;
  data: TruckGeolocationDto;
}
