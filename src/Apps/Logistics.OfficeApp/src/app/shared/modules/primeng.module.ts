import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockModule } from 'primeng/dock';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext'
import { AutoCompleteModule } from 'primeng/autocomplete';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { MessagesModule } from 'primeng/messages';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    CardModule,
    DockModule,
    InputTextModule,
    ProgressSpinnerModule,
    AutoCompleteModule,
    DropdownModule,
    TableModule,
    MessagesModule,
    MessageModule,
    ToastModule,
    TooltipModule,
    ConfirmDialogModule,
  ],
  exports: [
    CardModule,
    DockModule,
    InputTextModule,
    ProgressSpinnerModule,
    AutoCompleteModule,
    DropdownModule,
    TableModule,
    MessagesModule,
    MessageModule,
    ToastModule,
    TooltipModule,
    ConfirmDialogModule
  ],
  providers: [
    MessageService,
    ConfirmationService
  ]
})
export class PrimengModule { }
