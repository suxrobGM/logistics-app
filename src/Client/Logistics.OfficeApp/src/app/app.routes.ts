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
    path: 'dashboard',
    loadChildren: () => import('./pages/dashboard').then((m) => m.DASHBOARD_ROUTES),
    data: {
      breadcrumb: 'Dashboard',
    },
  },
  {
    path: 'employees',
    loadChildren: () => import('./pages/employee').then((m) => m.EMPLOYEE_ROUTES),
    data: {
      breadcrumb: 'Employees',
    },
  },
  {
    path: 'loads',
    loadChildren: () => import('./pages/load').then((m) => m.LOAD_ROUTES),
    data: {
      breadcrumb: 'Loads',
    },
  },
  {
    path: 'trucks',
    loadChildren: () => import('./pages/truck').then((m) => m.TRUCK_ROUTES),
    data: {
      breadcrumb: 'Trucks',
    },
  },
  {
    path: 'customers',
    loadChildren: () => import('./pages/customer').then((m) => m.CUSTOMER_ROUTES),
    data: {
      breadcrumb: 'Customers',
    },
  },
  {
    path: 'accounting',
    loadChildren: () => import('./pages/accounting').then((m) => m.ACCOUNTING_ROUTES),
    data: {
      breadcrumb: '',
    },
  },
  {
    path: '**',
    redirectTo: '404',
  },
];
