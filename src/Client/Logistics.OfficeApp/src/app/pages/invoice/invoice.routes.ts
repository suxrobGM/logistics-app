import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {EmployeePayrollInvoicesListComponent} from "./employee-payroll-invoices-list/employee-payroll-invoices-list.component";
import {LoadInvoiceDetailsComponent} from "./load-invoice-details/load-invoice-details.component";
import {LoadInvoicesListComponent} from "./load-invoices-list/load-invoices-list.component";
import {PayrollInvoiceEditComponent} from "./payroll-invoice-edit/payroll-invoice-edit.component";
import {PayrollInvoicesListComponent} from "./payroll-invoices-list/payroll-invoices-list.component";

export const invoiceRoutes: Routes = [
  {
    path: "loads",
    component: LoadInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices",
      permission: Permissions.Payments.View,
    },
  },
  {
    path: "loads/:loadId",
    component: LoadInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices Details",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "loads/:loadId/:invoiceId",
    component: LoadInvoiceDetailsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoice Details",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "payroll",
    component: PayrollInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoices",
      permission: Permissions.Invoices.View,
    },
  },
  {
    path: "payroll/:invoiceId/edit",
    component: PayrollInvoiceEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll Invoice",
      permission: Permissions.Payrolls.View,
    },
  },
  {
    path: "payroll/add",
    component: PayrollInvoiceEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll Invoice",
      permission: Permissions.Payrolls.View,
    },
  },
  {
    path: "payroll/employee/:employeeId",
    component: EmployeePayrollInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Payroll Invoices",
      permission: Permissions.Payrolls.Create,
    },
  },
];
