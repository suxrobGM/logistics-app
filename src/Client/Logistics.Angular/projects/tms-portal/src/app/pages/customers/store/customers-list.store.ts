import { getCustomers } from "@logistics/shared/api";
import type { CustomerDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the customers list page.
 */
export const CustomersListStore = createListStore<CustomerDto>(getCustomers, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
