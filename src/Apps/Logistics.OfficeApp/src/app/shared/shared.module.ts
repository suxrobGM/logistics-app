import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimengModule } from './primeng.module';
import { NavDockComponent } from './components/nav-dock/nav-dock.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { DistanceUnitPipe } from './pipes';
import { DateUtils } from './utils';

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
  ],
  providers: [
    DistanceUnitPipe,
    DateUtils
  ]
})
export class SharedModule { }
