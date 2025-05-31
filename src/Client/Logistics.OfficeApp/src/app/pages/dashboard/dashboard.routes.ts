import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {DashboardComponent} from "./dashboard.component";

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
