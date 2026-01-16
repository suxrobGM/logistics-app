import { createListStore } from "@logistics/shared/stores";
import { getTrips } from "@/core/api";
import type { TripDto } from "@/core/api/models";

/**
 * Store for the trips list page.
 */
export const TripsListStore = createListStore<TripDto>(getTrips, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
