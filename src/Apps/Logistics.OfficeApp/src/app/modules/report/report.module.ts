import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './primeng.module';
import { SharedModule } from '@shared/shared.module';
import { ReportRoutingModule } from './report-routing.module';
import { TruckReportComponent } from './pages/truck-report/truck-report.component';
import { ReportPageComponent } from './pages/overview/overviewcomponent';

@NgModule({
  declarations: [
    TruckReportComponent,
    ReportPageComponent
  ],
  imports: [
    CommonModule,
    ReportRoutingModule,
    PrimengModule,
    SharedModule
  ]
})
export class ReportModule { }
