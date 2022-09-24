import { NgModule } from '@angular/core';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@NgModule({
  exports: [
    BreadcrumbModule,
    ProgressSpinnerModule
  ]
})
export class PrimengModule { }
