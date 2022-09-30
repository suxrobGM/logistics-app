import { NgModule } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TableModule } from 'primeng/table';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';

@NgModule({
  exports: [
    AutoCompleteModule,
    CardModule,
    DropdownModule,
    ProgressSpinnerModule,
    InputTextModule,
    TableModule,
    DialogModule
  ]
})
export class PrimengModule { }
