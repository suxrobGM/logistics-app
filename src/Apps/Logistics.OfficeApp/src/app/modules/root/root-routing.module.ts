import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth';
import {
  Error404Component,
  UnauthorizedComponent,
  ForbiddenComponent,
  HomeComponent
} from './pages';

const rootRoutes: Routes = [
  {
    path: '', 
    component: HomeComponent
  },
  {
    path: 'forbidden',
    component: ForbiddenComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ['app.admin', 'tenant.owner', 'tenant.manager', 'tenant.dispatcher']
    }
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent
  },
  {
    path: '404',
    component: Error404Component
  },
  {
    path: '**',
    redirectTo: '404'
  }
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class RootRoutingModule {}