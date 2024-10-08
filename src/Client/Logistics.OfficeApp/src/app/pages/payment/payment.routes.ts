import {Routes} from "@angular/router";
import {ProcessPaymentComponent} from "./process-payment/process-payment.component";

export const paymentRoutes: Routes = [
  {
    path: "invoice/:invoiceId",
    component: ProcessPaymentComponent,
    data: {
      breadcrumb: "Invoice Payment",
    },
  },
  {
    path: "payroll/:payrollId",
    component: ProcessPaymentComponent,
    data: {
      breadcrumb: "Payroll Payment",
    },
  },
];
