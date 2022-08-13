import { NgModule } from '@angular/core';
import { AuthModule } from 'angular-auth-oidc-client';

@NgModule({
  imports: [
    AuthModule.forRoot({
      config: {
        authority: 'https://localhost:7001',
        postLoginRoute: '/dashboard',
        forbiddenRoute: '/forbidden',
        unauthorizedRoute: '/unauthorized',
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: 'logistics.officeapp',
        scope: 'openid profile offline_access roles logistics.api.tenant',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        renewTimeBeforeTokenExpiresInSeconds: 30,
      }
    })
  ],
  exports: [AuthModule],
})
export class AuthConfigModule { }
