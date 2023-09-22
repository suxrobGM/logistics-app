import {Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {DashboardComponent} from './dashboard.component';


export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    component: DashboardComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Main',
      permission: Permissions.Report.View,
    },
  },
];
