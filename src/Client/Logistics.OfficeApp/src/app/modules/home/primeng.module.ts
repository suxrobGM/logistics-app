import {NgModule} from '@angular/core';
import {CardModule} from 'primeng/card';
import {ChartModule} from 'primeng/chart';
import {TableModule} from 'primeng/table';
import {SkeletonModule} from 'primeng/skeleton';

@NgModule({
  exports: [
    CardModule,
    TableModule,
    ChartModule,
    SkeletonModule,
  ],
})
export class PrimengModule { }
