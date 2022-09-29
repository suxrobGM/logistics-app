import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Permissions } from '@shared/types';
import { AuthGuard } from '../auth/auth.guard';
import { ReportPageComponent } from './pages/overview/overview.component';
import { TruckReportComponent } from './pages/truck-report/truck-report.component';

const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'overview',
    pathMatch: 'full'
  },
  { 
    path: 'overview', 
    component: ReportPageComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Overview',
      permission: Permissions.Report.View
    }
  },
  { 
    path: 'truck/:id', 
    component: TruckReportComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Truck',
      permission: Permissions.Report.View
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class ReportRoutingModule {}