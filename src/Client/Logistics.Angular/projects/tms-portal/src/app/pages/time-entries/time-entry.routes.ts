import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { TimeEntriesList } from "./time-entries-list/time-entries-list";

export const timeEntryRoutes: Routes = [
  {
    path: "",
    component: TimeEntriesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Time Entries",
      permission: Permission.Payroll.View,
    },
  },
  {
    path: "employee/:employeeId",
    component: TimeEntriesList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Employee Time Entries",
      permission: Permission.Payroll.View,
    },
  },
];
