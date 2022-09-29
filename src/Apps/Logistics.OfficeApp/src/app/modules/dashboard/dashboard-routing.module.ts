import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Permissions } from '@shared/types';
import { AuthGuard } from '../auth/auth.guard';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

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