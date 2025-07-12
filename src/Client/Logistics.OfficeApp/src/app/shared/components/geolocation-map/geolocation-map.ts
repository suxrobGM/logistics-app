import {CommonModule} from "@angular/common";
import {Component, input, signal} from "@angular/core";
import {MarkerComponent, NgxMapboxGLModule} from "ngx-mapbox-gl";
import {TruckGeolocationDto} from "@/core/api/models";
import {environment} from "@/env";
import {AddressPipe} from "@/shared/pipes";

@Component({
  selector: "app-geolocation-map",
  templateUrl: "./geolocation-map.html",
  styleUrls: [],
  standalone: true,
  imports: [CommonModule, NgxMapboxGLModule, AddressPipe],
})
export class GeolocationMap {
  protected readonly accessToken = environment.mapboxToken;
  protected readonly selectedMarker = signal<Marker | null>(null);

  public readonly geolocationData = input<TruckGeolocationDto[]>([]);
  public readonly zoom = input<number>(3);
  public readonly center = input<[number, number]>([-95, 35]);
  public readonly width = input("100%");
  public readonly height = input("100%");

  onSelectMarker(geoData: TruckGeolocationDto, markerComponent: MarkerComponent): void {
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
