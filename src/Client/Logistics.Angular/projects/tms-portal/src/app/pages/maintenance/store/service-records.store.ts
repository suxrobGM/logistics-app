import { getMaintenanceRecords } from "@logistics/shared/api";
import type { MaintenanceRecordDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the maintenance service records list page.
 */
export const ServiceRecordsStore = createListStore<MaintenanceRecordDto>(getMaintenanceRecords, {
  defaultSortField: "ServiceDate",
  defaultPageSize: 10,
});
