import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/shared.module';
import {PrimengModule} from './primeng.module';
import {DashboardRoutingModule} from './dashboard-routing.module';
import {TruckDashboardComponent, MainDashboardComponent} from './pages';


@NgModule({
  declarations: [
    TruckDashboardComponent,
    MainDashboardComponent,
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    PrimengModule,
    SharedModule,
  ],
})
export class DashboardModule { }
