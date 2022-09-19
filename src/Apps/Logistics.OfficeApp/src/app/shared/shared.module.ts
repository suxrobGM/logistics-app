import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './primeng.module';
import { DistanceUnitPipe } from './pipes';
import { DateUtils } from './utils';

@NgModule({
  declarations: [
    DistanceUnitPipe,
  ],
  imports: [
    CommonModule,
    PrimengModule
  ],
  exports: [
    PrimengModule,
    DistanceUnitPipe,
  ],
  providers: [
    DistanceUnitPipe,
    DateUtils
  ]
})
export class SharedModule { }
