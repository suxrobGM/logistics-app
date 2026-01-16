import { Component, input, signal } from "@angular/core";
import { AddressPipe } from "@logistics/shared/pipes";
import { MapComponent, MarkerComponent, PopupComponent } from "ngx-mapbox-gl";
import type { TruckGeolocationDto } from "@/core/api/models";

@Component({
  selector: "app-geolocation-map",
  templateUrl: "./geolocation-map.html",
  imports: [AddressPipe, MapComponent, MarkerComponent, PopupComponent],
})
export class GeolocationMap {
  protected readonly selectedMarker = signal<Marker | null>(null);

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
