import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { EmployeePayrollInvoicesList } from "./employee-payroll-invoices-list/employee-payroll-invoices-list";
import { InvoiceDashboard } from "./invoice-dashboard/invoice-dashboard";
import { LoadInvoiceDetails } from "./load-invoice-details/load-invoice-details";
import { LoadInvoicesList } from "./load-invoices-list/load-invoices-list";
import { PayrollDashboard } from "./payroll-dashboard/payroll-dashboard";
import { PayrollInvoiceDetails } from "./payroll-invoice-details/payroll-invoice-details";
import { PayrollInvoiceEdit } from "./payroll-invoice-edit/payroll-invoice-edit";
import { PayrollInvoicesList } from "./payroll-invoices-list/payroll-invoices-list";

export const invoiceRoutes: Routes = [
  {
    path: "",
    component: InvoiceDashboard,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Invoice Dashboard",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "loads",
    component: LoadInvoicesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices",
      permission: Permission.Payment.View,
    },
  },
  {
    path: "loads/:loadId",
    component: LoadInvoicesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices Details",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "loads/:loadId/:invoiceId",
    component: LoadInvoiceDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoice Details",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "payroll",
    component: PayrollInvoicesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoices",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "payroll/dashboard",
    component: PayrollDashboard,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Dashboard",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "payroll/add",
    component: PayrollInvoiceEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "payroll/employee/:employeeId",
    component: EmployeePayrollInvoicesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Payroll Invoices",
      permission: Permission.Payroll.Manage,
    },
  },
  {
    path: "payroll/:invoiceId",
    component: PayrollInvoiceDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payroll Invoice Details",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "payroll/:invoiceId/edit",
    component: PayrollInvoiceEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Payroll Invoice",
      permission: Permission.Payroll.View,
    },
  },
];
