import {Routes} from '@angular/router';
import {Permissions} from '@core/enums';
import {AuthGuard} from '@core/guards';
import {EditTruckComponent} from './edit-truck/edit-truck.component';
import {ListTruckComponent} from './list-trucks/list-trucks.component';
import {TruckDetailsComponent} from './truck-details/truck-details.component';


export const TRUCK_ROUTES: Routes = [
  {
    path: '',
    component: ListTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: '',
      permission: Permissions.Trucks.View,
    },
  },
  {
    path: 'add',
    component: EditTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Trucks.Create,
    },
  },
  {
    path: 'edit/:id',
    component: EditTruckComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Trucks.Edit,
    },
  },
  {
    path: 'details/:id',
    component: TruckDetailsComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Details',
      permission: Permissions.Trucks.View,
    },
  },
];
