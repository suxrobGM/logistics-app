import {Routes} from '@angular/router';
import {Error404Component} from '@pages/error404';
import {LoginComponent} from '@pages/login';
import {UnauthorizedComponent} from '@pages/unauthorized';


export const APP_ROUTES: Routes = [
  {
    path: '',
    component: LoginComponent,
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent,
  },
  {
    path: '404',
    component: Error404Component,
  },
  {
    path: 'home',
    loadChildren: () => import('./pages/home').then((m) => m.HOME_ROUTES),
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
  {
    path: '**',
    redirectTo: '404',
  },
];
