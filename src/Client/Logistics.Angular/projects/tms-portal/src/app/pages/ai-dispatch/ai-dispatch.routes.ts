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
  // Must stay above ":id" - routing is first-match-wins, or "policy" reads as a session id.
  {
    path: "policy",
    loadComponent: () =>
      import("./dispatch-policy/dispatch-policy").then((m) => m.DispatchPolicyPage),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Policy",
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
