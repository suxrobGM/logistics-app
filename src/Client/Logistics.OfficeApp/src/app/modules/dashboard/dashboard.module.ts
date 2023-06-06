import {NgModule} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {SharedModule} from '@shared/shared.module';
import {PrimengModule} from './primeng.module';
import {DashboardRoutingModule} from './dashboard-routing.module';
import {DashboardPageComponent} from './pages';

@NgModule({
  declarations: [
    DashboardPageComponent,
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    SharedModule,
    PrimengModule,
  ],
  providers: [
    CurrencyPipe,
  ],
})
export class DashboardModule { }
