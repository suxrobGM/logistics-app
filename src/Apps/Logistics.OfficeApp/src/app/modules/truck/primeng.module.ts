import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TableModule } from 'primeng/table';

@NgModule({
  exports: [
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    AutoCompleteModule,
  ]
})
export class PrimengModule { }
