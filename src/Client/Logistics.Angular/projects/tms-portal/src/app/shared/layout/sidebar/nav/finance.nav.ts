import { Permission } from "@logistics/shared";
import type { NavSection } from "@/shared/layout/nav-menu";

export const financeNav: NavSection = {
  id: "finance",
  label: "Finance",
  items: [
    {
      id: "payroll",
      label: "Payroll",
      icon: "wallet",
      feature: "payroll",
      permission: Permission.Payroll.View,
      children: [
        {
          id: "payroll-dashboard",
          label: "Dashboard",
          route: "/payroll",
          permission: Permission.Payroll.View,
        },
        {
          id: "payroll-invoices",
          label: "Invoices",
          route: "/payroll/invoices",
          permission: Permission.Invoice.View,
        },
        {
          id: "payroll-timesheets",
          label: "Timesheets",
          route: "/timesheets",
          feature: "timesheets",
          permission: Permission.Payroll.View,
        },
      ],
    },
    {
      id: "invoicing",
      label: "Invoicing",
      icon: "file-pen-line",
      feature: "invoices",
      permission: Permission.Invoice.View,
      children: [
        {
          id: "invoicing-dashboard",
          label: "Dashboard",
          route: "/invoices",
          permission: Permission.Invoice.View,
        },
        {
          id: "invoicing-loads",
          label: "Load Invoices",
          route: "/invoices/loads",
          permission: Permission.Invoice.View,
        },
      ],
    },
    {
      id: "expenses",
      label: "Expenses",
      icon: "banknote",
      feature: "expenses",
      permission: Permission.Expense.View,
      children: [
        {
          id: "expenses-all",
          label: "All Expenses",
          route: "/expenses",
          permission: Permission.Expense.View,
        },
        {
          id: "expenses-analytics",
          label: "Analytics",
          route: "/expenses/analytics",
          permission: Permission.Expense.View,
        },
      ],
    },
    {
      id: "fuel-cards",
      label: "Fuel Cards",
      icon: "fuel",
      feature: "fuel_cards",
      permission: Permission.FuelCard.View,
      children: [
        {
          id: "fuel-cards-transactions",
          label: "Transactions",
          route: "/fuel-cards",
          permission: Permission.FuelCard.View,
        },
        {
          id: "fuel-cards-providers",
          label: "Providers",
          route: "/fuel-cards/providers",
          permission: Permission.FuelCard.Manage,
        },
      ],
    },
  ],
};
