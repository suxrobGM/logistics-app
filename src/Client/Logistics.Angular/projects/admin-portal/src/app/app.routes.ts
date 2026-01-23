import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";

export const appRoutes: Routes = [
  {
    path: "home",
    loadChildren: () => import("./pages/home/home.routes").then((m) => m.homeRoutes),
  },
  {
    path: "demo-requests",
    loadChildren: () =>
      import("./pages/demo-requests/demo-requests.routes").then((m) => m.demoRequestsRoutes),
    canActivate: [authGuard],
  },
  {
    path: "contact-submissions",
    loadChildren: () =>
      import("./pages/contact-submissions/contact-submissions.routes").then(
        (m) => m.contactSubmissionsRoutes,
      ),
    canActivate: [authGuard],
  },
  {
    path: "tenants",
    loadChildren: () => import("./pages/tenants/tenant.routes").then((m) => m.tenantRoutes),
    canActivate: [authGuard],
  },
  {
    path: "subscriptions",
    loadChildren: () =>
      import("./pages/subscriptions/subscription.routes").then((m) => m.subscriptionRoutes),
    canActivate: [authGuard],
  },
  {
    path: "subscription-plans",
    loadChildren: () => import("./pages/plans/plan.routes").then((m) => m.planRoutes),
    canActivate: [authGuard],
  },
  {
    path: "users",
    loadChildren: () => import("./pages/users/user.routes").then((m) => m.userRoutes),
    canActivate: [authGuard],
  },
  {
    path: "blog-posts",
    loadChildren: () => import("./pages/blog-posts/blog-posts.routes").then((m) => m.blogPostsRoutes),
    canActivate: [authGuard],
  },
  {
    path: "",
    loadComponent: () => import("./pages/login/login").then((m) => m.Login),
  },
  {
    path: "unauthorized",
    loadComponent: () => import("./pages/unauthorized/unauthorized").then((m) => m.Unauthorized),
  },
  {
    path: "**",
    redirectTo: "",
  },
];
