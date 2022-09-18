import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';

@NgModule({
  exports: [
    AutoCompleteModule,
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    InputTextModule,
  ]
})
export class PrimengModule { }
