import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";

export const payrollRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./dashboard/payroll-dashboard").then((m) => m.PayrollDashboard),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Dashboard",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "invoices",
    loadComponent: () =>
      import("./invoices/list/payroll-invoices-list").then((m) => m.PayrollInvoicesList),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoices",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "invoices/add",
    loadComponent: () =>
      import("./invoices/add/payroll-invoice-add").then((m) => m.PayrollInvoiceAdd),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "invoices/:invoiceId",
    loadComponent: () =>
      import("./invoices/details/payroll-invoice-details").then((m) => m.PayrollInvoiceDetails),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoice Details",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "invoices/:invoiceId/edit",
    loadComponent: () =>
      import("./invoices/edit/payroll-invoice-edit").then((m) => m.PayrollInvoiceEdit),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "employee/:employeeId",
    loadComponent: () =>
      import("./employee/employee-payroll-invoices").then((m) => m.EmployeePayrollInvoices),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Payroll Invoices",
      permission: Permission.Payroll.Manage,
    },
  },
];
