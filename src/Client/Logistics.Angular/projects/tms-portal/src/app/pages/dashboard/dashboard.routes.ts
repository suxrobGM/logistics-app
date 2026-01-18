import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { DashboardComponent } from "./dashboard";

export const dashboardRoutes: Routes = [
  {
    path: "",
    component: DashboardComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Main",
      permission: Permission.Stat.View,
    },
  },
];
