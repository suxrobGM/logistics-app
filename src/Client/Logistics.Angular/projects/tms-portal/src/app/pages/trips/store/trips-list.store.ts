import { getTrips, type TripDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the trips list page.
 */
export const TripsListStore = createListStore<TripDto>(getTrips, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
