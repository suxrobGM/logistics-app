import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {ManageSubscriptionComponent} from "./manage-subscription/manage-subscription";
import {RenewSubscriptionComponent} from "./renew-subscription/renew-subscription";
import {ViewPlansComponent} from "./view-plans/view-plans";

export const subscriptionRoutes: Routes = [
  {
    path: "manage",
    component: ManageSubscriptionComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Manage Subscription",
    },
  },
  {
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
