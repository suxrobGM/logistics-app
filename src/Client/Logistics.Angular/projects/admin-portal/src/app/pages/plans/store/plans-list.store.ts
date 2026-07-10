import { getSubscriptionPlans, type SubscriptionPlanDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the subscription plans list page.
 */
export const PlansListStore = createListStore<SubscriptionPlanDto>(getSubscriptionPlans, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
