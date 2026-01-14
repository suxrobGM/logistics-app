import { getCustomers } from "@/core/api";
import type { CustomerDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the customers list page.
 */
export const CustomersListStore = createListStore<CustomerDto>(getCustomers, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
