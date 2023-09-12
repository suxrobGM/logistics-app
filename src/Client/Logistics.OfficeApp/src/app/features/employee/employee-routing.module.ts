import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {Permissions} from 'src/app/core/types';
import {AuthGuard} from '@core/guards';
import {AddEmployeeComponent} from './add-employee/add-employee.component';
import {EditEmployeeComponent} from './edit-employee/edit-employee.component';
import {ListEmployeeComponent} from './list-employee/list-employee.component';


const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    component: ListEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      permission: Permissions.Employee.View,
    },
  },
  {
    path: 'add',
    component: AddEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Employee.Create,
    },
  },
  {
    path: 'edit/:id',
    component: EditEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Employee.Edit,
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}
