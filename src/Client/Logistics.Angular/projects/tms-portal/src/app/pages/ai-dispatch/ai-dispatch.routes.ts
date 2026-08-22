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
    path: "rate-floors",
    loadComponent: () => import("./rate-floors/rate-floors").then((m) => m.RateFloors),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Rate Floors",
      permission: Permission.Negotiation.View,
    },
  },
  {
    path: "negotiations",
    loadComponent: () => import("./negotiations/negotiations").then((m) => m.Negotiations),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Negotiations",
      permission: Permission.Negotiation.View,
    },
  },
  {
    path: "negotiations/:id",
    loadComponent: () =>
      import("./negotiation-details/negotiation-details").then((m) => m.NegotiationDetails),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Thread",
      permission: Permission.Negotiation.View,
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
