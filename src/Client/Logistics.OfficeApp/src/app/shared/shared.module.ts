import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {PrimengModule} from './primeng.module';
import {DistanceUnitPipe} from './pipes';
import {BreadcrumbComponent, GeolocationMapComponent} from './components';
import {AppConfig} from '../configs/app.config';


@NgModule({
  declarations: [
    GeolocationMapComponent,
    BreadcrumbComponent,
    DistanceUnitPipe,
  ],
  imports: [
    CommonModule,
    PrimengModule,
    NgxMapboxGLModule.withConfig({
      accessToken: AppConfig.mapboxToken,
    }),
  ],
  exports: [
    PrimengModule,
    DistanceUnitPipe,
    BreadcrumbComponent,
    GeolocationMapComponent,
  ],
  providers: [
    DistanceUnitPipe,
  ],
})
export class SharedModule { }
