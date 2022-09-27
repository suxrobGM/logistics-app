import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserRole } from '@shared/types';
import { AuthGuard } from '../auth/auth.guard';
import { EditTruckComponent } from './pages/edit-truck/edit-truck.component';
import { ListTruckComponent } from './pages/list-truck/list-truck.component';

const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full'
  },
  { 
    path: 'list', 
    component: ListTruckComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager, UserRole.Dispatcher]
    }
  },
  {
    path: 'add', 
    component: EditTruckComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager]
    }
  },
  {
    path: 'edit/:id', 
    component: EditTruckComponent, 
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
export class TruckRoutingModule {}