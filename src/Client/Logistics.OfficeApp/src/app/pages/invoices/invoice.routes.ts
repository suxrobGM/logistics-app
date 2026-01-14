import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permission } from "@/shared/models";
import { EmployeePayrollInvoicesListComponent } from "./employee-payroll-invoices-list/employee-payroll-invoices-list";
import { LoadInvoiceDetailsComponent } from "./load-invoice-details/load-invoice-details";
import { LoadInvoicesListComponent } from "./load-invoices-list/load-invoices-list";
import { PayrollInvoiceEditComponent } from "./payroll-invoice-edit/payroll-invoice-edit";
import { PayrollInvoicesListComponent } from "./payroll-invoices-list/payroll-invoices-list";

export const invoiceRoutes: Routes = [
  {
    path: "loads",
    component: LoadInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices",
      permission: Permission.Payment.View,
    },
  },
  {
    path: "loads/:loadId",
    component: LoadInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices Details",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "loads/:loadId/:invoiceId",
    component: LoadInvoiceDetailsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoice Details",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "payroll",
    component: PayrollInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoices",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "payroll/:invoiceId/edit",
    component: PayrollInvoiceEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "payroll/add",
    component: PayrollInvoiceEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "payroll/employee/:employeeId",
    component: EmployeePayrollInvoicesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Payroll Invoices",
      permission: Permission.Payroll.Manage,
    },
  },
];
