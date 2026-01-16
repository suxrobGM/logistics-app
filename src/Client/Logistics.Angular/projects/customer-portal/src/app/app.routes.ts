import type { Routes } from "@angular/router";

import { authGuard, tenantGuard } from "@/core/auth";
import { MainLayout } from "@/layout";

export const routes: Routes = [
  {
    path: "login",
    loadComponent: () => import("./pages/login/login").then((m) => m.Login),
  },
  {
    path: "unauthorized",
    loadComponent: () => import("./pages/errors/unauthorized").then((m) => m.Unauthorized),
  },
  {
    path: "select-tenant",
    canActivate: [authGuard],
    loadComponent: () => import("./pages/select-tenant/select-tenant").then((m) => m.SelectTenant),
  },
  {
    path: "",
    component: MainLayout,
    canActivate: [authGuard, tenantGuard],
    children: [
      {
        path: "",
        loadComponent: () => import("./pages/dashboard/dashboard").then((m) => m.Dashboard),
      },
      {
        path: "shipments",
        loadComponent: () =>
          import("./pages/shipments/shipments-list").then((m) => m.ShipmentsList),
      },
      {
        path: "shipments/:id",
        loadComponent: () =>
          import("./pages/shipments/shipment-details").then((m) => m.ShipmentDetails),
      },
      {
        path: "invoices",
        loadComponent: () => import("./pages/invoices/invoices-list").then((m) => m.InvoicesList),
      },
      {
        path: "documents",
        loadComponent: () =>
          import("./pages/documents/documents-list").then((m) => m.DocumentsList),
      },
      {
        path: "account",
        loadComponent: () =>
          import("./pages/account/account-settings").then((m) => m.AccountSettings),
      },
    ],
  },
  {
    path: "**",
    loadComponent: () => import("./pages/errors/not-found").then((m) => m.NotFound),
  },
];
