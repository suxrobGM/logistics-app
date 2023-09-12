import {NgModule} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {AppConfig} from '@core/constants/app.config';
import {SharedModule} from '@shared/shared.module';
import {PrimengModule} from './primeng.module';
import {HomeRoutingModule} from './home-routing.module';
import {OverviewComponent} from './overview/overview.component';


@NgModule({
  declarations: [
    OverviewComponent,
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    SharedModule,
    PrimengModule,
    NgxMapboxGLModule.withConfig({
      accessToken: AppConfig.mapboxToken,
    }),
  ],
  providers: [
    CurrencyPipe,
  ],
})
export class HomeModule { }
