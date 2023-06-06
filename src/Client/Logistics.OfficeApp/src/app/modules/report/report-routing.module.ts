import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {Permissions} from '@shared/types';
import {AuthGuard} from '../auth';
import {OverviewComponent, TruckReportComponent} from './pages';

const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'overview',
    pathMatch: 'full',
  },
  {
    path: 'overview',
    component: OverviewComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Overview',
      permission: Permissions.Report.View,
    },
  },
  {
    path: 'truck/:id',
    component: TruckReportComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Truck',
      permission: Permissions.Report.View,
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class ReportRoutingModule {}
