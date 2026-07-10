import { getDemoRequests, type DemoRequestDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the demo requests list page.
 */
export const DemoRequestsListStore = createListStore<DemoRequestDto>(getDemoRequests, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
