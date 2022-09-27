import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserRole } from '@shared/types';
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
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager, UserRole.Dispatcher]
    }
  },
  { 
    path: 'add', 
    component: EditLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager, UserRole.Dispatcher]
    }
  },
  { 
    path: 'edit/:id', 
    component: EditLoadComponent, 
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      roles: [UserRole.AppAdmin, UserRole.Owner, UserRole.Manager, UserRole.Dispatcher]
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class LoadRoutingModule {}