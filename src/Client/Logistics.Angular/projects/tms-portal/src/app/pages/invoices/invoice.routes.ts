import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { InvoiceDashboard } from "./invoice-dashboard/invoice-dashboard";
import { LoadInvoiceDetails } from "./load-invoice-details/load-invoice-details";

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
    redirectTo: "",
    pathMatch: "full",
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
