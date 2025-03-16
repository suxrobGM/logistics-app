import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {EditPaymentComponent} from "./edit-payment/edit-payment.component";
import {EditPayrollComponent} from "./edit-payroll/edit-payroll.component";
import {ListInvoicesComponent} from "./list-invoices/list-invoices.component";
import {ListPaymentsComponent} from "./list-payments/list-payments.component";
import {ListPayrollComponent} from "./list-payroll/list-payroll.component";
import {ViewEmployeePayrollsComponent} from "./view-employee-payrolls/view-employee-payrolls.component";
import {ViewInvoiceComponent} from "./view-invoice/view-invoice.component";

export const accountingRoutes: Routes = [
  {
    path: "payments",
    component: ListPaymentsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payments",
      permission: Permissions.Payments.View,
    },
  },
  {
    path: "payments/add",
    component: EditPaymentComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Payments.Create,
    },
  },
  {
    path: "payments/edit/:id",
    component: EditPaymentComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Payments.Edit,
    },
  },
  {
    path: "invoices",
    component: ListInvoicesComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Invoices",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "invoices/view/:id",
    component: ViewInvoiceComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "View Invoice",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "payrolls",
    component: ListPayrollComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payrolls",
      permission: Permissions.Payrolls.View,
    },
  },
  {
    path: "payrolls/add",
    component: EditPayrollComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll",
      permission: Permissions.Payrolls.Create,
    },
  },
  {
    path: "payrolls/edit/:id",
    component: EditPayrollComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll",
      permission: Permissions.Payrolls.Edit,
    },
  },
  {
    path: "employee-payrolls/:employeeId",
    component: ViewEmployeePayrollsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "View Employee Payrolls",
      permission: Permissions.Payrolls.View,
    },
  },
];
