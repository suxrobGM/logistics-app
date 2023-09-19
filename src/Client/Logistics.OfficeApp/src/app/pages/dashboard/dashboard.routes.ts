import {Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {MainDashboardComponent} from './main-dashboard/main-dashboard.component';
import {TruckDashboardComponent} from './truck-dashboard/truck-dashboard.component';


export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'main',
    pathMatch: 'full',
  },
  {
    path: 'main',
    component: MainDashboardComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Main',
      permission: Permissions.Report.View,
    },
  },
  {
    path: 'truck/:id',
    component: TruckDashboardComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Truck',
      permission: Permissions.Report.View,
    },
  },
];
