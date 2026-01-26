import { getLoads } from "@logistics/shared/api";
import type { LoadDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the loads list page.
 */
export const LoadsListStore = createListStore<LoadDto>(getLoads, {
  defaultSortField: "-DispatchedAt",
  defaultPageSize: 10,
});
