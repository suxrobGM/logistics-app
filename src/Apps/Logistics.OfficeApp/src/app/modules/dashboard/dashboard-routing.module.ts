import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';;
import { AutoLoginAllRoutesGuard, AutoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';
import { AuthGuard } from '../auth/auth.guard';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

const rootRoutes: Routes = [
  { path: 'dashboard', component: DashboardPageComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutingModule {}