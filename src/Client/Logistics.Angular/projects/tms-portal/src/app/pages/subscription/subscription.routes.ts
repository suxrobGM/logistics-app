import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { RenewSubscriptionComponent } from "./renew-subscription/renew-subscription";
import { ViewPlansComponent } from "./view-plans/view-plans";

export const subscriptionRoutes: Routes = [
  {
    // Deliberately unguarded: `authGuard` requires an active subscription, so this is the only
    // route a lapsed tenant can reach to fix billing. Do not add a guard here.
    path: "renew",
    component: RenewSubscriptionComponent,
    data: {
      breadcrumb: "Renew Subscription",
    },
  },
  {
    path: "plans",
    component: ViewPlansComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "View Subscription Plans",
    },
  },
];
