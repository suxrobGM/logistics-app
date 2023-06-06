import {NgModule} from '@angular/core';
import {AuthConfig} from '@configs';
import {AuthModule} from 'angular-auth-oidc-client';

@NgModule({
  imports: [
    AuthModule.forRoot({
      config: AuthConfig,
    }),
  ],
  exports: [AuthModule],
})
export class AuthConfigModule { }
