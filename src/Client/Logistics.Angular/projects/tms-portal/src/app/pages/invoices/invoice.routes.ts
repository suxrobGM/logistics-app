import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { InvoiceDashboard } from "./invoice-dashboard/invoice-dashboard";
import { LoadInvoiceDetails } from "./load-invoice-details/load-invoice-details";
import { LoadInvoicesList } from "./load-invoices-list/load-invoices-list";

export const invoiceRoutes: Routes = [
  {
    path: "",
    component: InvoiceDashboard,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoice Dashboard",
      permission: Permission.Invoice.View,
    },
  },
  {
    path: "loads",
    component: LoadInvoicesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Load Invoices",
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
];
