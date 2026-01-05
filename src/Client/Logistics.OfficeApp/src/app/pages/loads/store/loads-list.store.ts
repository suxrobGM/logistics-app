import { getLoads$Json } from "@/core/api";
import type { LoadDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the loads list page.
 */
export const LoadsListStore = createListStore<LoadDto>(getLoads$Json, {
  defaultSortField: "-DispatchedAt",
  defaultPageSize: 10,
});
