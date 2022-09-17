import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TruckComponent } from './pages/truck/truck.component';
import { StatsRoutingModule } from './stats-routing.module';

@NgModule({
  declarations: [
    TruckComponent
  ],
  imports: [
    CommonModule,
    StatsRoutingModule
  ]
})
export class StatsModule { }
