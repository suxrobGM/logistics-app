import { getDriverBehaviorEvents, type DriverBehaviorEventDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the driver behavior events list page.
 */
export const DriverBehaviorListStore = createListStore<DriverBehaviorEventDto>(
  getDriverBehaviorEvents,
  {
    defaultSortField: "OccurredAt",
    defaultPageSize: 25,
  },
);
