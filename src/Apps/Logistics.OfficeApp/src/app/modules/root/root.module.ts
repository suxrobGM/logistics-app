import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Error404PageComponent } from './pages/error404-page/error404-page.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { RootRoutingModule } from './root-routing.module';
import { NavDockComponent } from './components/nav-dock/nav-dock.component';
import { DockModule } from 'primeng/dock'


@NgModule({
  declarations: [
    Error404PageComponent,
    HomePageComponent,
    NavDockComponent,
  ],
  imports: [
    CommonModule,
    RootRoutingModule,
    DockModule
  ]
})
export class RootModule { }
