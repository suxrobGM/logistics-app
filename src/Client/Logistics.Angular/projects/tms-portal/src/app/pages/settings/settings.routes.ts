import type { Routes } from "@angular/router";
import { featureGuardFromData, Permission } from "@logistics/shared";
import { settingsGuard } from "@/core/auth";

export const settingsRoutes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./settings-layout/settings-layout").then((m) => m.SettingsLayoutComponent),
    // Per child, with the child's snapshot - so `data.permission` is enforced and a new tab
    // can't be added ungated.
    canActivateChild: [settingsGuard],
    children: [
      {
        path: "",
        redirectTo: "company",
        pathMatch: "full",
      },
      {
        path: "company",
        loadComponent: () =>
          import("./company-settings/company-settings").then((m) => m.CompanySettingsComponent),
        data: { breadcrumb: "Company", permission: Permission.Tenant.View },
      },
      {
        path: "billing",
        loadComponent: () =>
          import("./billing-settings/billing-settings").then((m) => m.BillingSettings),
        data: { breadcrumb: "Billing", permission: Permission.Payment.View },
      },
      {
        path: "integrations",
        loadComponent: () =>
          import("./integrations-settings/integrations-settings").then(
            (m) => m.IntegrationsSettings,
          ),
        canActivate: [featureGuardFromData],
        data: {
          breadcrumb: "Integrations",
          permission: Permission.Accounting.View,
          feature: ["accounting", "mcp_server"],
        },
      },
      {
        path: "features",
        loadComponent: () =>
          import("./feature-settings/feature-settings").then((m) => m.FeatureSettingsComponent),
        data: { breadcrumb: "Features", permission: Permission.Tenant.View },
      },
    ],
  },
];
