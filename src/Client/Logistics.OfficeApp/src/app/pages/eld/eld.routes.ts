import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permission } from "@/shared/models";
import { EldDashboardComponent } from "./eld-dashboard/eld-dashboard";

export const eldRoutes: Routes = [
  {
    path: "",
    component: EldDashboardComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Eld.View,
    },
  },
];
