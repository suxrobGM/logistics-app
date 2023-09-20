import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import {NgFor} from '@angular/common';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {GeolocationData} from '@core/models';


@Component({
  selector: 'app-geolocation-map',
  templateUrl: './geolocation-map.component.html',
  styles: [],
  standalone: true,
  imports: [NgxMapboxGLModule, NgFor],
})
export class GeolocationMapComponent implements OnInit, OnChanges {
  @Input() geolocationData: GeolocationData[] = [];
  @Input() initialCenter: [number, number] = [-95, 35];


  ngOnInit(): void {}
  ngOnChanges(changes: SimpleChanges): void {}

  trackByFn(index: number, item: GeolocationData): string {
    return item.userId;
  }
}
