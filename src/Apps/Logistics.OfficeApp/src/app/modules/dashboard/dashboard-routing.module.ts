import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';;
import { AuthGuard } from '../auth/auth.guard';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

const rootRoutes: Routes = [
  { 
    path: 'dashboard', 
    component: DashboardPageComponent, 
    canActivate: [AuthGuard], 
    data: {
      roles: ['admin', 'owner', 'dispatcher']
    } 
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutingModule {}