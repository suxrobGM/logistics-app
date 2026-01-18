import type { Routes } from "@angular/router";
import { SubscriptionAdd } from "./subscription-add/subscription-add";
import { SubscriptionsList } from "./subscriptions-list/subscriptions-list";

export const subscriptionRoutes: Routes = [
  {
    path: "",
    component: SubscriptionsList,
    data: {
      breadcrumb: "Subscriptions",
    },
  },
  {
    path: "add",
    component: SubscriptionAdd,
    data: {
      breadcrumb: "Add",
    },
  },
];
