import type { Routes } from "@angular/router";

export const licenseRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./product-license/product-license").then((m) => m.ProductLicense),
    data: {
      breadcrumb: "License",
    },
  },
];
