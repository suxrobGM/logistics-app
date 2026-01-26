import { getExpenses } from "@logistics/shared/api";
import type { ExpenseDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the expenses list page.
 */
export const ExpensesListStore = createListStore<ExpenseDto>(getExpenses, {
  defaultSortField: "-ExpenseDate",
  defaultPageSize: 10,
});
