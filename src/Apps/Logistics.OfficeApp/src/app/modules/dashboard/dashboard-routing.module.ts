import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Permissions } from '@shared/types';
import { AuthGuard } from '../auth';
import { DashboardPageComponent } from './pages';

const rootRoutes: Routes = [
  { 
    path: '', 
    component: DashboardPageComponent, 
    canActivate: [AuthGuard], 
    data: {
      breadcrumb: 'Dashboard',
      permission: Permissions.Load.View
    } 
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutingModule {}