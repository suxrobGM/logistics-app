import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {EditLoadComponent} from './edit-load/edit-load.component';
import {ListLoadComponent} from './list-load/list-load.component';


const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    component: ListLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      permission: Permissions.Load.View,
    },
  },
  {
    path: 'add',
    component: EditLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Load.Create,
    },
  },
  {
    path: 'edit/:id',
    component: EditLoadComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Load.Edit,
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class LoadRoutingModule {}
