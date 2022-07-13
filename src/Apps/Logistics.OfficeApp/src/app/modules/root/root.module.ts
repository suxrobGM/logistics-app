import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Error404PageComponent } from './pages/error404-page/error404-page.component';
import { RootRoutingModule } from './root-routing.module';
import { SharedModule } from '@app/shared/shared.module';

@NgModule({
  declarations: [
    Error404PageComponent
  ],
  imports: [
    CommonModule,
    RootRoutingModule,
    SharedModule
  ]
})
export class RootModule { }
