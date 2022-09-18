import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { RootRoutingModule } from './root-routing.module';
import { Error404Component } from './pages/error404/error404.component';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';
import { ForbiddenComponent } from './pages/forbidden/forbidden.component';
import { HomeComponent } from './pages/home/home.component';

@NgModule({
  declarations: [
    Error404Component,
    UnauthorizedComponent,
    ForbiddenComponent,
    HomeComponent
  ],
  imports: [
    CommonModule,
    RootRoutingModule,
    SharedModule
  ]
})
export class RootModule { }
