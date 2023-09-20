import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import {NgFor, NgIf} from '@angular/common';
import {MarkerComponent, NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {GeolocationData} from '@core/models';


@Component({
  selector: 'app-geolocation-map',
  templateUrl: './geolocation-map.component.html',
  styles: [],
  standalone: true,
  imports: [
    NgxMapboxGLModule,
    NgFor,
    NgIf,
  ],
})
export class GeolocationMapComponent implements OnInit, OnChanges {
  public selectedMarker: Marker | null = null;

  @Input() geolocationData: GeolocationData[] = [];
  @Input() initialCenter: [number, number] = [-95, 35];

  ngOnInit(): void {}
  ngOnChanges(changes: SimpleChanges): void {}

  trackByFn(index: number, item: GeolocationData): string {
    return item.userId;
  }

  onSelectMarker(geoData: GeolocationData, markerComponent: MarkerComponent): void {
    this.selectedMarker = {
      component: markerComponent,
      data: geoData,
    };
  }
}

interface Marker {
  component: MarkerComponent;
  data: GeolocationData;
}
