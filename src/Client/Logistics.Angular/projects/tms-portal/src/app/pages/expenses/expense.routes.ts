import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { ExpensesListPage } from "./expenses-list/expenses-list";
import { ExpenseAddPage } from "./expense-add/expense-add";
import { ExpenseAnalyticsPage } from "./expense-analytics/expense-analytics";

export const expenseRoutes: Routes = [
  {
    path: "",
    component: ExpensesListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Expense.View,
    },
  },
  {
    path: "add",
    component: ExpenseAddPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Expense.Manage,
    },
  },
  {
    path: "analytics",
    component: ExpenseAnalyticsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Analytics",
      permission: Permission.Expense.View,
    },
  },
];
