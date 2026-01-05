import { getCustomers$Json } from "@/core/api";
import type { CustomerDto } from "@/core/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the customers list page.
 */
export const CustomersListStore = createListStore<CustomerDto>(getCustomers$Json, {
  defaultSortField: "Name",
  defaultPageSize: 10,
});
