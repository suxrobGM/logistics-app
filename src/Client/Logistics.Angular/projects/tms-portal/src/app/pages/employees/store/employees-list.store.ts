import { getEmployees } from "@logistics/shared/api";
import type { EmployeeDto } from "@logistics/shared/api/models";
import { createListStore } from "@/shared/stores";

/**
 * Store for the employees list page.
 */
export const EmployeesListStore = createListStore<EmployeeDto>(getEmployees, {
  defaultSortField: "FirstName",
  defaultPageSize: 10,
});
