import {
  Component,
  Input,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MarkerComponent, NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {TruckGeolocationDto} from '@core/models';
import {AddressPipe} from '@core/pipes';


@Component({
  selector: 'app-geolocation-map',
  templateUrl: './geolocation-map.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    NgxMapboxGLModule,
    AddressPipe,
  ],
})
export class GeolocationMapComponent {
  public selectedMarker: Marker | null = null;

  @Input({required: true}) accessToken!: string;
  @Input() geolocationData: TruckGeolocationDto[];
  @Input() zoom: number;
  @Input() center: [number, number];
  @Input() width: string;
  @Input() height: string;

  constructor() {
    this.geolocationData = [];
    this.zoom = 3;
    this.center = [-95, 35];
    this.width = '100%';
    this.height = '100%';
  }

  onSelectMarker(geoData: TruckGeolocationDto, markerComponent: MarkerComponent): void {
    this.selectedMarker = {
      component: markerComponent,
      data: geoData,
    };
  }
}

interface Marker {
  component: MarkerComponent;
  data: TruckGeolocationDto;
}
