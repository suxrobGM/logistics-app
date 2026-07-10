import { getSubscriptions, type SubscriptionDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the subscriptions list page.
 */
export const SubscriptionsListStore = createListStore<SubscriptionDto>(getSubscriptions, {
  defaultSortField: "-Status",
  defaultPageSize: 10,
});
