import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockModule } from 'primeng/dock';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext'
import { AutoCompleteModule } from 'primeng/autocomplete';
import { DropdownModule } from 'primeng/dropdown';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    CardModule,
    DockModule,
    InputTextModule,
    ProgressSpinnerModule,
    AutoCompleteModule,
    DropdownModule
  ],
  exports: [
    CardModule,
    DockModule,
    InputTextModule,
    ProgressSpinnerModule,
    AutoCompleteModule,
    DropdownModule
  ]
})
export class PrimengModule { }
