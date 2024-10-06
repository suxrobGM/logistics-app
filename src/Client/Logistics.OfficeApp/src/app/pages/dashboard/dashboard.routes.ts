import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {authGuard} from "@/core/guards";
import {DashboardComponent} from "./dashboard.component";

export const DASHBOARD_ROUTES: Routes = [
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
