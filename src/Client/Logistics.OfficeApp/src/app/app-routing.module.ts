import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';


const APP_ROUTES: Routes = [
  {
    path: 'home',
    loadChildren: () => import('./pages/home/home.module').then((m) => m.HomeModule),
  },
  {
    path: 'employees',
    loadChildren: () => import('./pages/employee/employee.module').then((m) => m.EmployeeModule),
    data: {
      breadcrumb: 'Employees',
    },
  },
  {
    path: 'loads',
    loadChildren: () => import('./pages/load/load.module').then((m) => m.LoadModule),
    data: {
      breadcrumb: 'Loads',
    },
  },
  {
    path: 'trucks',
    loadChildren: () => import('./pages/truck/truck.module').then((m) => m.TruckModule),
    data: {
      breadcrumb: 'Trucks',
    },
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./pages/dashboard/dashboard.module').then((m) => m.DashboardModule),
    data: {
      breadcrumb: 'Dashboard',
    },
  },
];

@NgModule({
  imports: [RouterModule.forRoot(APP_ROUTES)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
