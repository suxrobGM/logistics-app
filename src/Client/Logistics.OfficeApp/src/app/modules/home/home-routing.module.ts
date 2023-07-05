import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {Permissions} from '@shared/types';
import {AuthGuard} from '../auth';
import {OverviewComponent} from './pages/overview/overview.component';

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
