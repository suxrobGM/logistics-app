import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";

export const reportsRoutes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./reports-layout/reports.layout").then((m) => m.ReportsLayoutComponent),
    children: [
      {
        path: "loads",
        loadComponent: () =>
          import("./loads-report/loads-report").then((m) => m.LoadsReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Stat.View,
        },
      },
      {
        path: "drivers",
        loadComponent: () =>
          import("./drivers-report/drivers-dashboard/drivers-report").then(
            (m) => m.DriversReportComponent,
          ),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Stat.View,
        },
      },
      {
        path: "drivers/detailed",
        loadComponent: () =>
          import("./drivers-report/drivers-detailed/drivers-detailed").then(
            (m) => m.DriversDetailedComponent,
          ),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Stat.View,
        },
      },
      {
        path: "financials",
        loadComponent: () =>
          import("./financials-report/financials-report").then((m) => m.FinancialsReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Stat.View,
        },
      },
      {
        path: "payroll",
        loadComponent: () =>
          import("./payroll-report/payroll-report").then((m) => m.PayrollReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Payroll.View,
        },
      },
      {
        path: "safety",
        loadComponent: () =>
          import("./safety-report/safety-report").then((m) => m.SafetyReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Safety.View,
        },
      },
      {
        path: "maintenance",
        loadComponent: () =>
          import("./maintenance-report/maintenance-report").then((m) => m.MaintenanceReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permission.Truck.View,
        },
      },
      { path: "", redirectTo: "loads", pathMatch: "full" },
    ],
  },
];
