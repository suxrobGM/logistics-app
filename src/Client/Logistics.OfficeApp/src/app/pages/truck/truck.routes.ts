import {Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {EditTruckComponent} from './edit-truck/edit-truck.component';
import {ListTruckComponent} from './list-truck/list-truck.component';
import {TruckDetailsComponent} from './truck-details/truck-details.component';


export const TRUCK_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    component: ListTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      permission: Permissions.Truck.View,
    },
  },
  {
    path: 'add',
    component: EditTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Truck.Create,
    },
  },
  {
    path: 'edit/:id',
    component: EditTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Truck.Edit,
    },
  },
  {
    path: 'details/:id',
    component: TruckDetailsComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Details',
      permission: Permissions.Truck.View,
    },
  },
];
