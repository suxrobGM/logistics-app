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
      children: [
        {
          id: "payroll-dashboard",
          label: "Dashboard",
          route: "/payroll",
        },
        {
          id: "payroll-invoices",
          label: "Invoices",
          route: "/payroll/invoices",
        },
        {
          id: "payroll-timesheets",
          label: "Timesheets",
          route: "/timesheets",
          feature: "timesheets",
        },
      ],
    },
    {
      id: "invoicing",
      label: "Invoicing",
      icon: "file-pen-line",
      feature: "invoices",
      children: [
        {
          id: "invoicing-dashboard",
          label: "Dashboard",
          route: "/invoices",
        },
        {
          id: "invoicing-loads",
          label: "Load Invoices",
          route: "/invoices/loads",
        },
      ],
    },
    {
      id: "expenses",
      label: "Expenses",
      icon: "banknote",
      feature: "expenses",
      children: [
        {
          id: "expenses-all",
          label: "All Expenses",
          route: "/expenses",
        },
        {
          id: "expenses-analytics",
          label: "Analytics",
          route: "/expenses/analytics",
        },
      ],
    },
    {
      id: "fuel-cards",
      label: "Fuel Cards",
      icon: "fuel",
      feature: "fuel_cards",
      children: [
        {
          id: "fuel-cards-transactions",
          label: "Transactions",
          route: "/fuel-cards",
        },
        {
          id: "fuel-cards-providers",
          label: "Providers",
          route: "/fuel-cards/providers",
        },
      ],
    },
  ],
};
