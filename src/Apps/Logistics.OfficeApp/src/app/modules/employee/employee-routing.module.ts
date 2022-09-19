import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
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
      roles: ['app.admin', 'tenant.owner', 'tenant.manager', 'tenant.dispatcher']
    }
  },
  { 
    path: 'add', 
    component: AddEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: ['app.admin', 'tenant.owner', 'tenant.manager']
    }
  },
  { 
    path: 'edit/:id', 
    component: EditEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      roles: ['app.admin', 'tenant.owner', 'tenant.manager']
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}