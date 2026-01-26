import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";

export const timesheetsRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./list/timesheets-list").then((m) => m.TimesheetsList),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Timesheets",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "employee/:employeeId",
    loadComponent: () => import("./list/timesheets-list").then((m) => m.TimesheetsList),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Timesheets",
      permission: Permission.Payroll.View,
    },
  },
];
