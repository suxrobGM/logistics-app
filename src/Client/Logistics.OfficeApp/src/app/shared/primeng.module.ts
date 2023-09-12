import {NgModule} from '@angular/core';
import {MessagesModule} from 'primeng/messages';
import {MessageModule} from 'primeng/message';
import {ToastModule} from 'primeng/toast';
import {TooltipModule} from 'primeng/tooltip';
import {MessageService} from 'primeng/api';
import {ButtonModule} from 'primeng/button';
import {BreadcrumbModule} from 'primeng/breadcrumb';

@NgModule({
  exports: [
    BreadcrumbModule,
    ButtonModule,
    MessagesModule,
    MessageModule,
    ToastModule,
    TooltipModule,
  ],
  providers: [
    MessageService,
  ],
})
export class PrimengModule { }
