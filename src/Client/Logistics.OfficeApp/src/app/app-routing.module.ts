import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';

const routes: Routes = [
  {
    path: 'home',
    loadChildren: () => import('./modules/home/home.module').then((m) => m.HomeModule),
  },
  {
    path: 'employees',
    loadChildren: () => import('./modules/employee/employee.module').then((m) => m.EmployeeModule),
    data: {
      breadcrumb: 'Employees',
    },
  },
  {
    path: 'loads',
    loadChildren: () => import('./modules/load/load.module').then((m) => m.LoadModule),
    data: {
      breadcrumb: 'Loads',
    },
  },
  {
    path: 'trucks',
    loadChildren: () => import('./modules/truck/truck.module').then((m) => m.TruckModule),
    data: {
      breadcrumb: 'Trucks',
    },
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./modules/dashboard/dashboard.module').then((m) => m.DashboardModule),
    data: {
      breadcrumb: 'Dashboard',
    },
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
