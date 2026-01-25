import { getTimeEntries } from "@logistics/shared/api";
import type { TimeEntryDto } from "@logistics/shared/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the time entries list page.
 */
export const TimeEntriesListStore = createListStore<TimeEntryDto>(getTimeEntries, {
  defaultSortField: "-Date",
  defaultPageSize: 25,
});
