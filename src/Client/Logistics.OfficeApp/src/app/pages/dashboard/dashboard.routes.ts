import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permissions } from "@/shared/models";
import { DashboardComponent } from "./dashboard";

export const dashboardRoutes: Routes = [
  {
    path: "",
    component: DashboardComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Main",
      permission: Permissions.Stats.View,
    },
  },
];
