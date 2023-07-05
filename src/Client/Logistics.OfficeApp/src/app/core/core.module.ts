import {AuthModule} from 'angular-auth-oidc-client';
import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HTTP_INTERCEPTORS} from '@angular/common/http';
import {TenantService} from './services/tenant.service';
import {AuthConfig} from './constants/auth.config';
import {TenantInterceptor} from './interceptors/tenant.interceptor';
import {TokenInterceptor} from './interceptors/token.interceptor';
import {AuthGuard} from './guards/auth.guard';

@NgModule({
  declarations: [],
  imports: [
    AuthModule.forRoot({
      config: AuthConfig,
    }),
    CommonModule,
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: TenantInterceptor, multi: true},
    {provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true},
    TenantService,
    AuthGuard,
  ],
})
export class CoreModule { }
