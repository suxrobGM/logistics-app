import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'reports/loads'
  },
  {
    path: 'reports',
    children: [
      { path: 'loads', loadComponent: () => import('./reports/loads/loads.component').then(m => m.LoadsComponent) },
      { path: 'drivers', loadComponent: () => import('./reports/drivers/drivers.component').then(m => m.DriversComponent) },
      { path: 'financials', loadComponent: () => import('./reports/financials/financials.component').then(m => m.FinancialsComponent) },
    ]
  }
];
