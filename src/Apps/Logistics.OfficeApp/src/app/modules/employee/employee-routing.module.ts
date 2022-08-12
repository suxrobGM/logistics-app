import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { AddEmployeeComponent } from './pages/add-employee/add-employee.component';
import { EditEmployeeComponent } from './pages/edit-employee/edit-employee.component';
import { ListEmployeeComponent } from './pages/list-employee/list-employee.component';

const rootRoutes: Routes = [
  { 
    path: '', 
    component: ListEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['main.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  },
  { 
    path: 'employees/add', 
    component: AddEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['main.admin', 'tenant.owner']
    }
  },
  { 
    path: 'employees/edit', 
    component: EditEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['main.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}