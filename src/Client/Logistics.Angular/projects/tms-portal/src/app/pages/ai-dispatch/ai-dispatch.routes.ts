import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";

export const aiDispatchRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./sessions-list/sessions-list").then((m) => m.SessionsListPage),
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Dispatch.View,
    },
  },
  {
    path: ":id",
    loadComponent: () =>
      import("./session-details/session-details").then((m) => m.SessionDetailsPage),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Session",
      permission: Permission.Dispatch.View,
    },
  },
];
