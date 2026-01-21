import type { Routes } from "@angular/router";

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
    data: { breadcrumb: "Company" },
  },
];
