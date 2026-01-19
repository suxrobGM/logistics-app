import type { MenuItem } from "@/shared/layout/panel-menu";

export const sidebarItems: MenuItem[] = [
  {
    label: "Home",
    icon: "pi pi-home text-3xl!",
    route: "/home",
  },
  {
    label: "Dashboard",
    icon: "pi pi-chart-bar text-3xl!",
    route: "/dashboard",
  },
  {
    label: "Messages",
    icon: "pi pi-comments text-3xl!",
    route: "/messages",
  },
  {
    label: "Loads",
    icon: "pi pi-server text-3xl!",
    items: [
      {
        label: "All Loads",
        route: "/loads",
      },
      {
        label: "Trips",
        route: "/trips",
      },
    ],
  },
  {
    label: "Reports",
    icon: "pi pi-chart-line text-3xl!",
    items: [
      {
        label: "Loads",
        route: "/reports/loads",
      },
      {
        label: "Drivers",
        route: "/reports/drivers",
      },
      {
        label: "Drivers Detailed",
        route: "/reports/drivers/detailed",
      },
      {
        label: "Financials",
        route: "/reports/financials",
      },
    ],
  },
  {
    label: "Trucks",
    icon: "pi pi-truck text-3xl!",
    route: "/trucks",
  },
  {
    label: "ELD / HOS",
    icon: "pi pi-clock text-3xl!",
    route: "/eld",
  },
  {
    label: "Employees",
    icon: "pi pi-users text-3xl!",
    route: "/employees",
  },
  {
    label: "Customers",
    icon: "pi pi-shop text-3xl!",
    route: "/customers",
  },
  {
    label: "Invoices",
    icon: "pi pi-receipt text-3xl!",
    items: [
      {
        label: "Payroll",
        route: "/invoices/payroll",
      },
      {
        label: "Load",
        route: "/invoices/loads",
      },
    ],
  },
  {
    label: "Expenses",
    icon: "pi pi-wallet text-3xl!",
    items: [
      {
        label: "All Expenses",
        route: "/expenses",
      },
      {
        label: "Add Expense",
        route: "/expenses/add",
      },
      {
        label: "Analytics",
        route: "/expenses/analytics",
      },
    ],
  },
  {
    label: "Inspections",
    icon: "pi pi-clipboard text-3xl!",
    route: "/inspections",
  },
  {
    label: "Subscription",
    icon: "pi pi-credit-card text-3xl!",
    route: "/subscription/manage",
  },
] as const;
