import { getTrucks } from "@logistics/shared/api";
import type { TruckDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the trucks list page.
 */
export const TrucksListStore = createListStore<TruckDto>(getTrucks, {
  defaultSortField: "Number",
  defaultPageSize: 10,
});
