import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import {NgFor, NgIf} from '@angular/common';
import {MarkerComponent, NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {TruckGeolocation} from '@core/models';


@Component({
  selector: 'app-geolocation-map',
  templateUrl: './geolocation-map.component.html',
  styleUrls: [],
  standalone: true,
  imports: [
    NgxMapboxGLModule,
    NgFor,
    NgIf,
  ],
})
export class GeolocationMapComponent implements OnChanges {
  public selectedMarker: Marker | null = null;

  @Input({required: true}) accessToken!: string;
  @Input() geolocationData: TruckGeolocation[];
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

  ngOnChanges(changes: SimpleChanges): void {}

  trackByFn(index: number, item: TruckGeolocation): string {
    return item.truckId;
  }

  onSelectMarker(geoData: TruckGeolocation, markerComponent: MarkerComponent): void {
    this.selectedMarker = {
      component: markerComponent,
      data: geoData,
    };
  }

  formatAddress(address: string): string {
    return address.substring(0, address.lastIndexOf(','));
  }
}

interface Marker {
  component: MarkerComponent;
  data: TruckGeolocation;
}
