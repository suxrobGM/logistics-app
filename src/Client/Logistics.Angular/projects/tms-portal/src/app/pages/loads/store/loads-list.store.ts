import { createListStore } from "@logistics/shared/stores";
import { getLoads } from "@/core/api";
import type { LoadDto } from "@/core/api/models";

/**
 * Store for the loads list page.
 */
export const LoadsListStore = createListStore<LoadDto>(getLoads, {
  defaultSortField: "-DispatchedAt",
  defaultPageSize: 10,
});
