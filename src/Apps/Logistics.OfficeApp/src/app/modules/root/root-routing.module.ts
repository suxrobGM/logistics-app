import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Error404PageComponent } from './pages/error404-page/error404-page.component';
import { RoutesConfig } from '@app/configs/routes.config';
import { UnauthorizedPageComponent } from './pages/unauthorized-page/unauthorized-page.component';

const routesNames = RoutesConfig.routesNames;

const rootRoutes: Routes = [
  { path: routesNames.error404, component: Error404PageComponent },
  { path: routesNames.unauthorized, component: UnauthorizedPageComponent },
  { path: '**', redirectTo: RoutesConfig.routes.error404 },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class RootRoutingModule {}