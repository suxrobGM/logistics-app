import { getTrucks$Json } from "@/core/api";
import type { TruckDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the trucks list page.
 */
export const TrucksListStore = createListStore<TruckDto>(getTrucks$Json, {
  defaultSortField: "Number",
  defaultPageSize: 10,
});
