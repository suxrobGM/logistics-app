import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PrimengModule} from './primeng.module';
import {DistanceUnitPipe} from './pipes';

@NgModule({
  declarations: [
    DistanceUnitPipe,
  ],
  imports: [
    CommonModule,
    PrimengModule,
  ],
  exports: [
    PrimengModule,
    DistanceUnitPipe,
  ],
  providers: [
    DistanceUnitPipe,
  ],
})
export class SharedModule { }
