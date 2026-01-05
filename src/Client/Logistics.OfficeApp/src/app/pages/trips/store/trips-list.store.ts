import { getTrips$Json } from "@/core/api";
import type { TripDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the trips list page.
 */
export const TripsListStore = createListStore<TripDto>(getTrips$Json, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
