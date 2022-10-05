import { NgModule } from '@angular/core';
import { BadgeModule } from 'primeng/badge';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DockModule } from 'primeng/dock';
import { TagModule } from 'primeng/tag';

@NgModule({
  exports: [
    BadgeModule,
    BreadcrumbModule,
    DockModule,
    ProgressSpinnerModule,
    TagModule
  ]
})
export class PrimengModule { }
