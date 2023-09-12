import {NgModule} from '@angular/core';
import {CardModule} from 'primeng/card';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {TableModule} from 'primeng/table';
import {InputTextModule} from 'primeng/inputtext';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';

@NgModule({
  exports: [
    AutoCompleteModule,
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    InputTextModule,
    ConfirmDialogModule,
  ],
  providers: [
    ConfirmationService,
  ],
})
export class PrimengModule { }
