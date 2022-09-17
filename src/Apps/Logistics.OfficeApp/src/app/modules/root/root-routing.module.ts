import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/auth.guard';
import { Error404Component } from './pages/error404/error404.component';
import { ForbiddenComponent } from './pages/forbidden/forbidden.component';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';

const rootRoutes: Routes = [
  { 
    path: 'forbidden', 
    component: ForbiddenComponent, 
    canActivate: [AuthGuard],
    data: {
      roles: ['app.admin', 'tenant.owner', 'tenant.manager', 'tenant.dispatcher']
    }
  },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: '404', component: Error404Component },
  { path: '**', redirectTo: '404' },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class RootRoutingModule {}