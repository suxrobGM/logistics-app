import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';;
import { RoutesConfig } from '@app/configs/routes.config';
import { AuthGuard } from '../auth/auth.guard';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

const routesNames = RoutesConfig.routesNames;

const rootRoutes: Routes = [
  { path: routesNames.dashboard, component: DashboardPageComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: routesNames.dashboard, pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutingModule {}