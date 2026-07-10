import { getLoads, type LoadDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the loads list page.
 */
export const LoadsListStore = createListStore<LoadDto>(getLoads, {
  defaultSortField: "-DispatchedAt",
  defaultPageSize: 10,
});
