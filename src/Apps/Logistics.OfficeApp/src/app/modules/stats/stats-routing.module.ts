import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { TruckComponent } from './pages/truck/truck.component';

const rootRoutes: Routes = [
  { 
    path: '', 
    component: TruckComponent, 
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