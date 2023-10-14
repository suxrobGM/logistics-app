import {Routes} from '@angular/router';
import {Permissions} from '@core/enums';
import {AuthGuard} from '@core/guards';
import {EditLoadComponent} from './edit-load/edit-load.component';
import {ListLoadComponent} from './list-loads/list-loads.component';
import {AddLoadComponent} from './add-load/add-load.component';


export const LOAD_ROUTES: Routes = [
  {
    path: '',
    component: ListLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: '',
      permission: Permissions.Loads.View,
    },
  },
  {
    path: 'add',
    component: AddLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Loads.Create,
    },
  },
  {
    path: 'edit/:id',
    component: EditLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Loads.Edit,
    },
  },
];
