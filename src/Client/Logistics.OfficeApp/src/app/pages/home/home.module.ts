import {NgModule} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {AppConfig} from '@configs';
import {HomeRoutingModule} from './home-routing.module';
import {HomeComponent} from './home.component';


@NgModule({
  imports: [
    CommonModule,
    HomeRoutingModule,
    NgxMapboxGLModule.withConfig({
      accessToken: AppConfig.mapboxToken,
    }),
    HomeComponent,
  ],
  providers: [
    CurrencyPipe,
  ],
})
export class HomeModule { }
