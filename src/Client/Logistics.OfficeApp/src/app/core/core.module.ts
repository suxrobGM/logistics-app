import {AuthModule} from 'angular-auth-oidc-client';
import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HTTP_INTERCEPTORS} from '@angular/common/http';
import {AuthConfig} from '@/configs';
import {TenantInterceptor} from './interceptors/tenant.interceptor';
import {TokenInterceptor} from './interceptors/token.interceptor';
import {AuthGuard} from './guards/auth.guard';
import {
  ApiService,
  CookieService,
  LiveTrackingService,
  NotificationService,
  StorageService,
  ToastService,
} from './services';
import {MessageService} from 'primeng/api';


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
    AuthGuard,
    ApiService,
    CookieService,
    StorageService,
    LiveTrackingService,
    NotificationService,
    MessageService,
    ToastService,
  ],
})
export class CoreModule { }
