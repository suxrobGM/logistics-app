import {NgModule} from '@angular/core';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {DockModule} from 'primeng/dock';
import {TagModule} from 'primeng/tag';

@NgModule({
  exports: [
    DockModule,
    ProgressSpinnerModule,
    TagModule,
  ],
})
export class PrimengModule { }
