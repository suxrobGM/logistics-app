import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {DashboardRoutingModule} from './dashboard-routing.module';
import {MainDashboardComponent} from './main-dashboard/main-dashboard.component';
import {TruckDashboardComponent} from './truck-dashboard/truck-dashboard.component';


@NgModule({
  imports: [
    CommonModule,
    DashboardRoutingModule,
    TruckDashboardComponent,
    MainDashboardComponent,
  ],
})
export class DashboardModule { }
