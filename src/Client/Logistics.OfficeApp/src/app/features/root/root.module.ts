import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/shared.module';
import {RootRoutingModule} from './root-routing.module';
import {PrimengModule} from './primeng.module';
import {Error404Component} from './error404/error404.component';
import {ForbiddenComponent} from './forbidden/forbidden.component';
import {LoginComponent} from './login/login.component';
import {UnauthorizedComponent} from './unauthorized/unauthorized.component';
import {NavDockComponent, TopbarComponent} from './components';

@NgModule({
  declarations: [
    Error404Component,
    UnauthorizedComponent,
    ForbiddenComponent,
    LoginComponent,
    NavDockComponent,
    TopbarComponent,
  ],
  imports: [
    CommonModule,
    RootRoutingModule,
    SharedModule,
    PrimengModule,
  ],
  exports: [
    NavDockComponent,
    TopbarComponent,
  ],
})
export class RootModule { }
