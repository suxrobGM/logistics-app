import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { TruckReportComponent } from './pages/truck-report/truck-report.component';

const rootRoutes: Routes = [
  { 
    path: 'truck/:id', 
    component: TruckReportComponent, 
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
export class ReportRoutingModule {}