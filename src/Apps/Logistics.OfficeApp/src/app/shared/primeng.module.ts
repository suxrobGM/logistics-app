import { NgModule } from '@angular/core';
import { MessagesModule } from 'primeng/messages';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';

@NgModule({
  exports: [
    ButtonModule,
    MessagesModule,
    MessageModule,
    ToastModule,
    TooltipModule
  ],
  providers: [
    MessageService
  ]
})
export class PrimengModule { }