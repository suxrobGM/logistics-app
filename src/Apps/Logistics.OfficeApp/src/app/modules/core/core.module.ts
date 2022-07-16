import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { CookieService } from './services/cookie.service';
import { TenantInterceptor } from './interceptors/tenant.interceptor';
import { TokenInterceptor } from './interceptors/token.interceptor';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    CookieService,
    { provide: HTTP_INTERCEPTORS, useClass: TenantInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true }
  ]
})
export class CoreModule { }
