import { createListStore } from "@logistics/shared/stores";
import { getEmployees } from "@/core/api";
import type { EmployeeDto } from "@/core/api/models";

/**
 * Store for the employees list page.
 */
export const EmployeesListStore = createListStore<EmployeeDto>(getEmployees, {
  defaultSortField: "FirstName",
  defaultPageSize: 10,
});
