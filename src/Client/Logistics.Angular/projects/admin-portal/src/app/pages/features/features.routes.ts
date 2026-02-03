import type { Routes } from "@angular/router";

export const featuresRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./default-features/default-features").then((m) => m.DefaultFeatures),
    data: {
      breadcrumb: "Default Features",
    },
  },
  {
    path: "tenant/:tenantId",
    loadComponent: () => import("./tenant-features/tenant-features").then((m) => m.TenantFeatures),
    data: {
      breadcrumb: "Tenant Features",
    },
  },
];
