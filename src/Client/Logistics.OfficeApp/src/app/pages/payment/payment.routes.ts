import {Routes} from '@angular/router';
import {MakePaymentComponent} from './make-payment/make-payment.component';


export const PAYMENT_ROUTES: Routes = [
  {
    path: 'invoice/:invoiceId',
    component: MakePaymentComponent,
    data: {
      breadcrumb: 'Invoice Payment',
    },
  },
  {
    path: 'payroll/:payrollId',
    component: MakePaymentComponent,
    data: {
      breadcrumb: 'Payroll Payment',
    },
  },
];
