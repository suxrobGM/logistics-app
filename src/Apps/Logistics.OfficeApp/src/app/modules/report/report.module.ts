import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './primeng.module';
import { SharedModule } from '@shared/shared.module';
import { TruckReportComponent } from './pages/truck-report/truck-report.component';
import { ReportRoutingModule } from './report-routing.module';

@NgModule({
  declarations: [
    TruckReportComponent
  ],
  imports: [
    CommonModule,
    ReportRoutingModule,
    PrimengModule,
    SharedModule
  ]
})
export class ReportModule { }
