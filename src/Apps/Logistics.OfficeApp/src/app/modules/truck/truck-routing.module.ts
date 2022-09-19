import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
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
      roles: ['app.admin', 'tenant.owner', 'tenant.manager', 'tenant.dispatcher']
    }
  },
  {
    path: 'add', 
    component: EditTruckComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: ['app.admin', 'tenant.owner', 'tenant.manager']
    }
  },
  {
    path: 'edit/:id', 
    component: EditTruckComponent, 
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
export class TruckRoutingModule {}