import {Routes} from "@angular/router";

export const reportsRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./components/reports-layout/reports.layout").then(m => m.ReportsLayoutComponent),
    children: [
      { path: "loads", loadComponent: () => import("./components/loads-report/loads-report").then(m => m.LoadsReportComponent) },
      { path: "drivers", loadComponent: () => import("./components/drivers-report/drivers-reports").then(m => m.DriversReportComponent) },
      { path: "financials", loadComponent: () => import("./components/financials-report/financials-report").then(m => m.FinancialsReportComponent) },
      { path: "", redirectTo: "loads", pathMatch: "full"},
    ]
  }
];


