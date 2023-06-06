import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AuthConfigModule} from './auth-config.module';
import {AuthGuard} from './auth.guard';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    AuthConfigModule,
  ],
  providers: [
    AuthGuard,
  ],
})
export class AuthModule { }
