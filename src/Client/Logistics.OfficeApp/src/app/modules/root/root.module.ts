import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from '@shared/shared.module';
import {RootRoutingModule} from './root-routing.module';
import {PrimengModule} from './primeng.module';
import {NavDockComponent, TopbarComponent} from './components';
import {Error404Component,
  ForbiddenComponent,
  LoginComponent,
  UnauthorizedComponent,
} from './pages';


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
