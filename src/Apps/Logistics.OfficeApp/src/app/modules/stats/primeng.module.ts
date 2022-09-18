import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';

@NgModule({
  exports: [
    CardModule,
    ProgressSpinnerModule,
    ChartModule,
    SkeletonModule
  ]
})
export class PrimengModule { }
