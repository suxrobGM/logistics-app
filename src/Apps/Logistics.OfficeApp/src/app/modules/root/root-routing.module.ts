import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AutoLoginAllRoutesGuard } from 'angular-auth-oidc-client';
import { AuthGuard } from '../auth/auth.guard';
import { Error404PageComponent } from './pages/error404-page/error404-page.component';
import { ForbiddenPageComponent } from './pages/forbidden-page/forbidden-page.component';
import { UnauthorizedPageComponent } from './pages/unauthorized-page/unauthorized-page.component';

const rootRoutes: Routes = [
  { path: '404', component: Error404PageComponent },
  { path: 'unauthorized', component: UnauthorizedPageComponent },
  { path: 'forbidden', component: ForbiddenPageComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '404' },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class RootRoutingModule {}