import {Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {HomeComponent} from './home.component';


export const HOME_ROUTES: Routes = [
  {
    path: '',
    component: HomeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Home',
      permission: Permissions.Loads.View,
    },
  },
];
