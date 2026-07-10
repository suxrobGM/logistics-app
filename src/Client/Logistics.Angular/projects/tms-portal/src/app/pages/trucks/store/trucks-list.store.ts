import { getTrucks, type TruckDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the trucks list page.
 */
export const TrucksListStore = createListStore<TruckDto>(getTrucks, {
  defaultSortField: "Number",
  defaultPageSize: 10,
});
