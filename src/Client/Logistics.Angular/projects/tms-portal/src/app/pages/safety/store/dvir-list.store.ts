import { getDvirReports, type DvirReportDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the DVIR reports list page.
 */
export const DvirListStore = createListStore<DvirReportDto>(getDvirReports, {
  defaultSortField: "InspectionDate",
  defaultPageSize: 10,
});
