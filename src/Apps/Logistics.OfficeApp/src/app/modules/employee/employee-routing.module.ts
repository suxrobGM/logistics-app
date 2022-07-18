import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { AddEmployeeComponent } from './pages/add-employee/add-employee.component';
import { EditEmployeeComponent } from './pages/edit-employee/edit-employee.component';
import { ListEmployeeComponent } from './pages/list-employee/list-employee.component';

const rootRoutes: Routes = [
  { 
    path: 'add-employee', 
    component: AddEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['admin', 'owner']
    }
  },
  { 
    path: 'edit-employee', 
    component: EditEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['admin', 'owner', 'dispatcher']
    }
  },
  { 
    path: 'list-employee', 
    component: ListEmployeeComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['admin', 'owner', 'dispatcher']
    }
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}