import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TruckStatsComponent } from './pages/truck-stats/truck-stats.component';
import { StatsRoutingModule } from './stats-routing.module';
import { PrimengModule } from './primeng.module';

@NgModule({
  declarations: [
    TruckStatsComponent
  ],
  imports: [
    CommonModule,
    StatsRoutingModule,
    PrimengModule
  ]
})
export class StatsModule { }
