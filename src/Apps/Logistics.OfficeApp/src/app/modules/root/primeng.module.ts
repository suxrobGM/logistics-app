import { NgModule } from '@angular/core';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DockModule } from 'primeng/dock';

@NgModule({
  exports: [
    BreadcrumbModule,
    DockModule,
    ProgressSpinnerModule
  ]
})
export class PrimengModule { }
