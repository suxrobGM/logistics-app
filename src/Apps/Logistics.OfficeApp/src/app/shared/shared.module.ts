import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './modules/primeng/primeng.module';
import { NavDockComponent } from './components/nav-dock/nav-dock.component';
import { TopbarComponent } from './components/topbar/topbar.component';

@NgModule({
  declarations: [
    NavDockComponent,
    TopbarComponent
  ],
  imports: [
    CommonModule,
    PrimengModule
  ],
  exports: [
    PrimengModule,
    NavDockComponent,
    TopbarComponent
  ]
})
export class SharedModule { }
