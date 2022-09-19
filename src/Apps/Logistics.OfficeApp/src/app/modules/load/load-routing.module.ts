import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { EditLoadComponent } from './pages/edit-load/edit-load.component';
import { ListLoadComponent } from './pages/list-load/list-load.component';

const rootRoutes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full'
  },
  {
    path: 'list', 
    component: ListLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'List',
      roles: ['app.admin', 'tenant.owner', 'tenant.manager', 'tenant.dispatcher']
    }
  },
  { 
    path: 'add', 
    component: EditLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: ['app.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  },
  { 
    path: 'edit/:id', 
    component: EditLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      roles: ['app.admin', 'tenant.owner', 'tenant.dispatcher']
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class LoadRoutingModule {}