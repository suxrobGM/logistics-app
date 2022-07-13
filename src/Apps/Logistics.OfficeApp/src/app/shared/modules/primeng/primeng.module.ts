import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockModule } from 'primeng/dock'


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    DockModule
  ],
  exports: [
    DockModule
  ]
})
export class PrimengModule { }
