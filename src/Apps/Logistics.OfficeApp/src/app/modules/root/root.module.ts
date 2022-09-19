import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { RootRoutingModule } from './root-routing.module';
import { Error404Component } from './pages/error404/error404.component';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';
import { ForbiddenComponent } from './pages/forbidden/forbidden.component';
import { HomeComponent } from './pages/home/home.component';
import { NavDockComponent } from './components/nav-dock/nav-dock.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { BreadcrumbComponent } from './components/breadcrumb/breadcrumb.component';
import { PrimengModule } from './primeng.module';

@NgModule({
  declarations: [
    Error404Component,
    UnauthorizedComponent,
    ForbiddenComponent,
    HomeComponent,
    NavDockComponent,
    TopbarComponent,
    BreadcrumbComponent
  ],
  imports: [
    CommonModule,
    RootRoutingModule,
    SharedModule,
    PrimengModule
  ],
  exports: [
    NavDockComponent,
    TopbarComponent,
    BreadcrumbComponent
  ]
})
export class RootModule { }
