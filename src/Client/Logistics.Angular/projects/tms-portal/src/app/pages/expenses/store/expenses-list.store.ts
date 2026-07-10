import { getExpenses, type ExpenseDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the expenses list page.
 */
export const ExpensesListStore = createListStore<ExpenseDto>(getExpenses, {
  defaultSortField: "-ExpenseDate",
  defaultPageSize: 10,
});
