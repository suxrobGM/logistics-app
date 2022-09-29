import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Permissions } from '@shared/types';
import { AuthGuard } from '../auth/auth.guard';
import { AddEmployeeComponent } from './pages/add-employee/add-employee.component';
import { EditEmployeeComponent } from './pages/edit-employee/edit-employee.component';
import { ListEmployeeComponent } from './pages/list-employee/list-employee.component';

const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full'
  },
  { 
    path: 'list',
    component: ListEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      permission: Permissions.Employee.View
    }
  },
  { 
    path: 'add', 
    component: AddEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Employee.Create
    }
  },
  { 
    path: 'edit/:id', 
    component: EditEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Employee.Edit
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}