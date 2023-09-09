import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PrimengModule} from './primeng.module';
import {DistanceUnitPipe} from './pipes';
import {
  ApiService,
  CookieService,
  StorageService,
} from './services';

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
    ApiService,
    CookieService,
    DistanceUnitPipe,
    StorageService,
  ],
})
export class SharedModule { }
