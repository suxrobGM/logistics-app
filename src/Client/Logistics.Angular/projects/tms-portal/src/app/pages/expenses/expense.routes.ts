import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { ExpensesListPage } from "./expenses-list/expenses-list";
import { ExpenseAddPage } from "./expense-add/expense-add";
import { ExpenseDetailPage } from "./expense-detail/expense-detail";
import { ExpenseEditPage } from "./expense-edit/expense-edit";
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
  {
    path: ":id",
    component: ExpenseDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Expense.View,
    },
  },
  {
    path: ":id/edit",
    component: ExpenseEditPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Expense.Manage,
    },
  },
];
