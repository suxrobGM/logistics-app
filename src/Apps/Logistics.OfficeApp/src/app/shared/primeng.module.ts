import { NgModule } from '@angular/core';
import { DockModule } from 'primeng/dock';
import { MessagesModule } from 'primeng/messages';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@NgModule({
  exports: [
    DockModule,
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
