import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {EditPayrollInvoiceComponent} from "./edit-payroll-invoice/edit-payroll-invoice.component";
import {ListLoadInvoicesComponent} from "./list-load-invoices/list-load-invoices.component";
import {ListPayrollInvoiceComponent} from "./list-payroll-invoices/list-payroll-invoices.component";
import {ViewEmployeePayrollInvoicesComponent} from "./view-employee-payroll-invoices/view-employee-payroll-invoices.component";
import {ViewLoadInvoiceComponent} from "./view-load-invoice/view-load-invoice.component";
import {ViewLoadInvoicesComponent} from "./view-load-invoices/view-load-invoices.component";

export const invoiceRoutes: Routes = [
  {
    path: "loads",
    component: ListLoadInvoicesComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices",
      permission: Permissions.Payments.View,
    },
  },
  {
    path: "loads/:id/view",
    component: ViewLoadInvoiceComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Virw Load Invoice",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "loads/:loadId/view",
    component: ViewLoadInvoicesComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Virw Load Invoices",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "payrolls",
    component: ListPayrollInvoiceComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoices",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "payrolls/:id/edit",
    component: EditPayrollInvoiceComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll Invoice",
      permission: Permissions.Payrolls.View,
    },
  },
  {
    path: "payrolls/add",
    component: EditPayrollInvoiceComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll Invoice",
      permission: Permissions.Payrolls.View,
    },
  },
  {
    path: "payrolls/employee/:employeeId/view",
    component: ViewEmployeePayrollInvoicesComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "View Employee Payroll Invoices",
      permission: Permissions.Payrolls.Create,
    },
  },
];
