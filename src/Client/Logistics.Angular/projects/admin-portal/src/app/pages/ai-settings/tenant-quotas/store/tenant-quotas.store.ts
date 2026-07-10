import { getTenantQuotaUsages, type TenantQuotaUsageDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the tenant AI quota usage list page.
 */
export const TenantQuotasStore = createListStore<TenantQuotaUsageDto>(getTenantQuotaUsages, {
  defaultSortField: "-UsedThisWeek",
});
