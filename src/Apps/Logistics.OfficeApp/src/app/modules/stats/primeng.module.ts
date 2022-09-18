import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@NgModule({
  exports: [
    CardModule,
    ProgressSpinnerModule,
  ]
})
export class PrimengModule { }
