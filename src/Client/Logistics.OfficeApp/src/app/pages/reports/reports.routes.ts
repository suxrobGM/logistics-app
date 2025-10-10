import { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permissions } from "@/shared/models";

export const reportsRoutes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./components/reports-layout/reports.layout").then((m) => m.ReportsLayoutComponent),
    children: [
      {
        path: "loads",
        loadComponent: () =>
          import("./components/loads-report/loads-report").then((m) => m.LoadsReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      {
        path: "drivers",
        loadComponent: () =>
          import("./components/drivers-report/drivers-dashboard/drivers-report").then(
            (m) => m.DriversReportComponent,
          ),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      {
        path: "drivers/detailed",
        loadComponent: () =>
          import("./components/drivers-report/drivers-detailed/drivers-detailed").then(
            (m) => m.DriversDetailedComponent,
          ),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      {
        path: "financials",
        loadComponent: () =>
          import("./components/financials-report/financials-report").then(
            (m) => m.FinancialsReportComponent,
          ),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      { path: "", redirectTo: "loads", pathMatch: "full" },
    ],
  },
];
