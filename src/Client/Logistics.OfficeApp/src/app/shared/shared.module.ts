import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PrimengModule} from './primeng.module';
import {DistanceUnitPipe} from './pipes';
import {BreadcrumbComponent} from './components';

@NgModule({
  declarations: [
    BreadcrumbComponent,
    DistanceUnitPipe,
  ],
  imports: [
    CommonModule,
    PrimengModule,
  ],
  exports: [
    PrimengModule,
    DistanceUnitPipe,
    BreadcrumbComponent,
  ],
  providers: [
    DistanceUnitPipe,
  ],
})
export class SharedModule { }
