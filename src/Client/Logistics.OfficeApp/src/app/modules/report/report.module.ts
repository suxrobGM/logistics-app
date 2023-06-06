import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/index';
import {PrimengModule} from './primeng.module';
import {ReportRoutingModule} from './report-routing.module';
import {OverviewComponent, TruckReportComponent} from './pages';

@NgModule({
  declarations: [
    TruckReportComponent,
    OverviewComponent,
  ],
  imports: [
    CommonModule,
    ReportRoutingModule,
    PrimengModule,
    SharedModule,
  ],
})
export class ReportModule { }
