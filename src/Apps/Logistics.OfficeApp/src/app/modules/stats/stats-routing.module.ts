import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { TruckStatsComponent } from './pages/truck-stats/truck-stats.component';

const rootRoutes: Routes = [
  { 
    path: 'truck/:id', 
    component: TruckStatsComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['app.admin', 'tenant.owner', 'tenant.manager']
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class StatsRoutingModule {}