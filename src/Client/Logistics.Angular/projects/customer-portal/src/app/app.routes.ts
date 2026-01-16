import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'shipments',
    loadComponent: () =>
      import('./pages/shipments/shipments-list').then((m) => m.ShipmentsList),
  },
  {
    path: 'shipments/:id',
    loadComponent: () =>
      import('./pages/shipments/shipment-details').then(
        (m) => m.ShipmentDetails
      ),
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/invoices/invoices-list').then((m) => m.InvoicesList),
  },
  {
    path: 'documents',
    loadComponent: () =>
      import('./pages/documents/documents-list').then((m) => m.DocumentsList),
  },
  {
    path: 'account',
    loadComponent: () =>
      import('./pages/account/account-settings').then((m) => m.AccountSettings),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
