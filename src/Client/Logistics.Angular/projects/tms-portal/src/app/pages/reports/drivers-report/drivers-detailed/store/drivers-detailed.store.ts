import { getDriversReport, type DriverReportDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the detailed drivers report page.
 *
 * No default sort field: the original table sent an empty `OrderBy` until the user
 * clicked a sortable column, so the store leaves sorting unset on load too.
 * The `StartDate` / `EndDate` date range is supplied at runtime via `setFilters`
 * (seeded on init and updated by the date range picker).
 */
export const DriversDetailedStore = createListStore<DriverReportDto>(getDriversReport, {
  defaultPageSize: 10,
});
