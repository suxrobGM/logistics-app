import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext'
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TableModule } from 'primeng/table';
import { DropdownModule } from 'primeng/dropdown';

@NgModule({
  exports: [
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    InputTextModule,
    AutoCompleteModule,
    DropdownModule
  ]
})
export class PrimengModule { }
