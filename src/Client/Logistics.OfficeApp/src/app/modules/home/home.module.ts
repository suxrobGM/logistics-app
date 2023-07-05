import {NgModule} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {SharedModule} from '@shared/shared.module';
import {PrimengModule} from './primeng.module';
import {HomeRoutingModule} from './home-routing.module';
import {OverviewComponent} from './pages/overview/overview.component';

@NgModule({
  declarations: [
    OverviewComponent,
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    SharedModule,
    PrimengModule,
  ],
  providers: [
    CurrencyPipe,
  ],
})
export class HomeModule { }
