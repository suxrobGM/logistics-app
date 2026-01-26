import { getTimeEntries } from "@logistics/shared/api";
import type { TimeEntryDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the timesheets list page.
 */
export const TimesheetsListStore = createListStore<TimeEntryDto>(getTimeEntries, {
  defaultSortField: "-Date",
  defaultPageSize: 25,
});
