import { getSubscriptionPlans } from "@logistics/shared/api";
import type { SubscriptionPlanDto } from "@logistics/shared/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the subscription plans list page.
 */
export const PlansListStore = createListStore<SubscriptionPlanDto>(getSubscriptionPlans, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
