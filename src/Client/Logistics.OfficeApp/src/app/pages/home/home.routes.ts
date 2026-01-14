import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permission } from "@/shared/models";
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
