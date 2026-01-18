import { getTenants } from "@logistics/shared/api";
import type { TenantDto } from "@logistics/shared/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the tenants list page.
 */
export const TenantsListStore = createListStore<TenantDto>(getTenants, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
