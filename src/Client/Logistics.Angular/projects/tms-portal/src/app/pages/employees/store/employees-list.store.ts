import { getEmployees, type EmployeeDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the employees list page.
 */
export const EmployeesListStore = createListStore<EmployeeDto>(getEmployees, {
  defaultSortField: "FirstName",
  defaultPageSize: 10,
});
