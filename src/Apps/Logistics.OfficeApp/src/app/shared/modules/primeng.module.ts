import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockModule } from 'primeng/dock';
import { CardModule } from 'primeng/card';
import { GMap, GMapModule } from 'primeng/gmap';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    CardModule,
    DockModule,
    GMapModule
  ],
  exports: [
    CardModule,
    DockModule,
    GMapModule
  ]
})
export class PrimengModule { }
