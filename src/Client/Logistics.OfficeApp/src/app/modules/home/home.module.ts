import {NgModule} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {AppConfig} from '@configs';
import {SharedModule} from '@shared/shared.module';

import {HomeRoutingModule} from './home-routing.module';
import {OverviewComponent} from './pages';


@NgModule({
  imports: [
    CommonModule,
    HomeRoutingModule,
    SharedModule,
    NgxMapboxGLModule.withConfig({
      accessToken: AppConfig.mapboxToken,
    }),
    OverviewComponent,
  ],
  providers: [
    CurrencyPipe,
  ],
})
export class HomeModule { }
