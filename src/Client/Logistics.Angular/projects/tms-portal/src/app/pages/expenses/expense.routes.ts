import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { ExpenseAddPage } from "./expense-add/expense-add";
import { ExpenseAnalyticsPage } from "./expense-analytics/expense-analytics";
import { ExpenseDetailsPage } from "./expense-details/expense-details";
import { ExpenseEditPage } from "./expense-edit/expense-edit";
import { ExpensesListPage } from "./expenses-list/expenses-list";

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
    component: ExpenseDetailsPage,
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
