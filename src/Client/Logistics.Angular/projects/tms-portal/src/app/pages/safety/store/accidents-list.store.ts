import { getAccidentReports, type AccidentReportDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the accident reports list page.
 */
export const AccidentsListStore = createListStore<AccidentReportDto>(getAccidentReports, {
  defaultSortField: "AccidentDate",
  defaultPageSize: 10,
});
