import { createListStore } from "@logistics/shared/stores";
import { getTrucks } from "@/core/api";
import type { TruckDto } from "@/core/api/models";

/**
 * Store for the trucks list page.
 */
export const TrucksListStore = createListStore<TruckDto>(getTrucks, {
  defaultSortField: "Number",
  defaultPageSize: 10,
});
