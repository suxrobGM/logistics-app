import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HTTP_INTERCEPTORS} from '@angular/common/http';
import {TenantInterceptor, TokenInterceptor} from './interceptors';
import {TenantService} from './services/tenant.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: TenantInterceptor, multi: true},
    {provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true},
    TenantService,
  ],
})
export class CoreModule { }
