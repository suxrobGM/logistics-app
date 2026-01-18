import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { HomeComponent } from "./home";

export const homeRoutes: Routes = [
  {
    path: "",
    component: HomeComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Home",
      permission: Permission.Load.View,
    },
  },
];
