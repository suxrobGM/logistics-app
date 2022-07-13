import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';;
import { RoutesConfig } from '@app/configs/routes.config';
import { DashboardPageComponent } from './pages/dashboard-page/dashboard-page.component';

const routesNames = RoutesConfig.routesNames;

const rootRoutes: Routes = [
  { path: routesNames.dashboard, component: DashboardPageComponent },
  { path: '', redirectTo: routesNames.dashboard, pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutingModule {}