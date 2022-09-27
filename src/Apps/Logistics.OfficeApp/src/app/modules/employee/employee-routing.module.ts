import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserRole } from '@shared/types';
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
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager, UserRole.Dispatcher]
    }
  },
  { 
    path: 'add', 
    component: AddEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager]
    }
  },
  { 
    path: 'edit/:id', 
    component: EditEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager]
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}