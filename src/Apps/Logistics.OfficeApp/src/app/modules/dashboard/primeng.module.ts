import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';

@NgModule({
  exports: [
    CardModule,
    TableModule,
    ChartModule,
  ]
})
export class PrimengModule { }
