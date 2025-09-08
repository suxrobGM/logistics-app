import {Routes} from "@angular/router";
import {Permissions} from "@/shared/models";
import {authGuard} from "@/core/auth";

export const reportsRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./components/reports-layout/reports.layout").then(m => m.ReportsLayoutComponent),
    children: [
      { 
        path: "loads",
         loadComponent: () => import("./components/loads-report/loads-report")
          .then(m => m.LoadsReportComponent), 
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      { 
        path: "drivers", 
        loadComponent: () => import("./components/drivers-report/drivers-dashboard")
          .then(m => m.DriversDashboardComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      { 
        path: "drivers/detailed", 
        loadComponent: () => import("./components/drivers-report/drivers-reports")
          .then(m => m.DriversReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      { 
        path: "financials", 
        loadComponent: () => import("./components/financials-report/financials-report")
          .then(m => m.FinancialsReportComponent),
        canActivate: [authGuard],
        data: {
          breadcrumb: "",
          permission: Permissions.Stats.View,
        },
      },
      { path: "", redirectTo: "loads", pathMatch: "full"},
    ]
  }
];


