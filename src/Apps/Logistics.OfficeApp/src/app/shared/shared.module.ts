import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './modules/primeng.module';
import { NavDockComponent } from './components/nav-dock/nav-dock.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { DistanceUnitPipe } from './pipes';

@NgModule({
  declarations: [
    NavDockComponent,
    TopbarComponent,
    DistanceUnitPipe,
  ],
  imports: [
    CommonModule,
    PrimengModule
  ],
  exports: [
    NavDockComponent,
    TopbarComponent,
    PrimengModule,
    DistanceUnitPipe,
  ]
})
export class SharedModule { }
