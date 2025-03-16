import {Routes} from "@angular/router";
import {authGuard} from "@/core/guards";
import {ManageSubscriptionComponent} from "./manage-subscription/manage-subscription.component";
import {RenewSubscriptionComponent} from "./renew-subscription/renew-subscription.component";

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
];
