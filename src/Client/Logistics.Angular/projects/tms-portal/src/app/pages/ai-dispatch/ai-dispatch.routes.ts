import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";

export const aiDispatchRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./dispatch-chat/dispatch-chat").then((m) => m.DispatchChat),
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Dispatch.View,
    },
  },
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
];
