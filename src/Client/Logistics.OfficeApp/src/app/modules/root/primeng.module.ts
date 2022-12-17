import { NgModule } from '@angular/core';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DockModule } from 'primeng/dock';
import { TagModule } from 'primeng/tag';

@NgModule({
  exports: [
    BreadcrumbModule,
    DockModule,
    ProgressSpinnerModule,
    TagModule
  ]
})
export class PrimengModule { }
