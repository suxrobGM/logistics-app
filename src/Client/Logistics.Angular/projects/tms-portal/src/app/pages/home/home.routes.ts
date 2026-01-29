import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { Home } from "./home";

export const homeRoutes: Routes = [
  {
    path: "",
    component: Home,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Home",
      permission: Permission.Load.View,
    },
  },
];
