import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/index';
import {RootRoutingModule} from './root-routing.module';
import {PrimengModule} from './primeng.module';
import {
  Error404Component,
  UnauthorizedComponent,
  ForbiddenComponent,
  LoginComponent,
} from './pages';
import {
  NavDockComponent,
  TopbarComponent,
  BreadcrumbComponent,
} from './components';

@NgModule({
  declarations: [
    Error404Component,
    UnauthorizedComponent,
    ForbiddenComponent,
    LoginComponent,
    NavDockComponent,
    TopbarComponent,
    BreadcrumbComponent,
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
    BreadcrumbComponent,
  ],
})
export class RootModule { }
