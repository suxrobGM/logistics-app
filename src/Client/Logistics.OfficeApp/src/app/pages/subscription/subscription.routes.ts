import {Routes} from "@angular/router";
import {ManageSubscriptionComponent} from "./manage/manage-subscription.component";

export const subscriptionRoutes: Routes = [
  {
    path: "manage",
    component: ManageSubscriptionComponent,
    data: {
      breadcrumb: "Manage Subscription",
    },
  },
];
