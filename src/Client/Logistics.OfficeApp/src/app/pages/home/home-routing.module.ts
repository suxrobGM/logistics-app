import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {Permissions} from '@core/helpers';
import {AuthGuard} from '@core/guards';
import {OverviewComponent} from './pages';

const rootRoutes: Routes = [
  {
    path: '',
    component: OverviewComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Overview',
      permission: Permissions.Load.View,
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(rootRoutes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}
