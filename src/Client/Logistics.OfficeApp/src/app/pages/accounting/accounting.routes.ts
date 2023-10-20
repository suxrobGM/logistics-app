import {Routes} from '@angular/router';
import {Permissions} from '@core/enums';
import {AuthGuard} from '@core/guards';
import {ListPaymentsComponent} from './list-payments/list-payments.component';
import {EditPaymentComponent} from './edit-payment/edit-payment.component';


export const ACCOUNTING_ROUTES: Routes = [
  {
    path: 'payments',
    component: ListPaymentsComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Payments',
      permission: Permissions.Loads.View,
    },
  },
  {
    path: 'payments/add',
    component: EditPaymentComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Add',
      permission: Permissions.Loads.Create,
    },
  },
  {
    path: 'payments/edit/:id',
    component: EditPaymentComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: 'Edit',
      permission: Permissions.Loads.Edit,
    },
  },
];
