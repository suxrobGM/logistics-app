import { getSubscriptions } from "@logistics/shared/api";
import type { SubscriptionDto } from "@logistics/shared/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the subscriptions list page.
 */
export const SubscriptionsListStore = createListStore<SubscriptionDto>(getSubscriptions, {
  defaultSortField: "-StartDate",
  defaultPageSize: 10,
});
