import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";

export const settingsRoutes: Routes = [
  {
    path: "",
    redirectTo: "company",
    pathMatch: "full",
  },
  {
    path: "company",
    loadComponent: () =>
      import("./company-settings/company-settings").then((m) => m.CompanySettingsComponent),
    canActivate: [authGuard],
    data: { breadcrumb: "Company" },
  },
  {
    path: "payments",
    loadComponent: () =>
      import("./payment-settings/payment-settings").then((m) => m.PaymentSettingsComponent),
    canActivate: [authGuard],
    data: { breadcrumb: "Payments" },
  },
  {
    path: "features",
    loadComponent: () =>
      import("./feature-settings/feature-settings").then((m) => m.FeatureSettingsComponent),
    canActivate: [authGuard],
    data: { breadcrumb: "Features" },
  },
];
