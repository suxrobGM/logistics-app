import {Routes} from "@angular/router";
import {authGuard} from "@/core/guards";
import {ProcessPaymentComponent} from "./process-payment/process-payment.component";

export const paymentRoutes: Routes = [
  {
    path: "invoice/:invoiceId",
    component: ProcessPaymentComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Invoice Payment",
    },
  },
  {
    path: "payroll/:payrollId",
    component: ProcessPaymentComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Payment",
    },
  },
];
