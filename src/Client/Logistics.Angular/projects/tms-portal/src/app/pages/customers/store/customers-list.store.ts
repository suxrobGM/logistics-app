import { createListStore } from "@logistics/shared/stores";
import { getCustomers } from "@/core/api";
import type { CustomerDto } from "@/core/api/models";

/**
 * Store for the customers list page.
 */
export const CustomersListStore = createListStore<CustomerDto>(getCustomers, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
