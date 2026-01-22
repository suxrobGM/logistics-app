import type { MenuItem } from "@/shared/layout/panel-menu";

export const sidebarItems: MenuItem[] = [
  {
    label: "Dashboard",
    icon: "pi pi-home text-3xl!",
    items: [
      {
        label: "Overview",
        route: "/dashboard",
      },
      {
        label: "Home",
        route: "/home",
      },
    ],
  },
  {
    label: "Messages",
    icon: "pi pi-comments text-3xl!",
    route: "/messages",
  },
  {
    label: "Operations",
    icon: "pi pi-server text-3xl!",
    items: [
      {
        label: "Loads",
        route: "/loads",
      },
      {
        label: "Trips",
        route: "/trips",
      },
      {
        label: "Load Board",
        route: "/loadboard",
      },
      {
        label: "Search Loads",
        route: "/loadboard/search",
      },
      {
        label: "Posted Trucks",
        route: "/loadboard/posted-trucks",
      },
      {
        label: "Providers",
        route: "/loadboard/providers",
      },
    ],
  },
  {
    label: "Fleet",
    icon: "pi pi-truck text-3xl!",
    items: [
      {
        label: "Trucks",
        route: "/trucks",
      },
      {
        label: "ELD / HOS",
        route: "/eld",
      },
      {
        label: "Inspections",
        route: "/inspections",
      },
    ],
  },
  {
    label: "Directory",
    icon: "pi pi-users text-3xl!",
    items: [
      {
        label: "Employees",
        route: "/employees",
      },
      {
        label: "Customers",
        route: "/customers",
      },
    ],
  },
  {
    label: "Finance",
    icon: "pi pi-wallet text-3xl!",
    items: [
      {
        label: "Payroll Invoices",
        route: "/invoices/payroll",
      },
      {
        label: "Load Invoices",
        route: "/invoices/loads",
      },
      {
        label: "Expenses",
        route: "/expenses",
      },
      {
        label: "Add Expense",
        route: "/expenses/add",
      },
      {
        label: "Expense Analytics",
        route: "/expenses/analytics",
      },
      {
        label: "Financial Report",
        route: "/reports/financials",
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
    ],
  },
  {
    label: "Settings",
    icon: "pi pi-cog text-3xl!",
    items: [
      {
        label: "Company",
        route: "/settings/company",
      },
      {
        label: "Subscription",
        route: "/subscription/manage",
      },
    ],
  },
] as const;
