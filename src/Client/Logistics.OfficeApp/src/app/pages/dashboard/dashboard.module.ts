import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/shared.module';

import {DashboardRoutingModule} from './dashboard-routing.module';
import {TruckDashboardComponent, MainDashboardComponent} from './pages';


@NgModule({
  imports: [
    CommonModule,
    DashboardRoutingModule,
    SharedModule,
    TruckDashboardComponent,
    MainDashboardComponent,
  ],
})
export class DashboardModule { }
