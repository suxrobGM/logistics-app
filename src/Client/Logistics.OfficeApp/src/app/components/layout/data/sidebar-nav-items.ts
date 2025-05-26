import {MenuItem} from "@/components/layout/panel-menu";

export const sidebarNavItems: MenuItem[] = [
  {
    label: "Home",
    icon: "bi bi-house-door h2",
    route: "/home",
  },
  {
    label: "Dashboard",
    icon: "bi bi-speedometer2 h2",
    route: "/dashboard",
  },
  {
    label: "Loads",
    icon: "bi bi-database h2",
    route: "/loads",
  },
  {
    label: "Trucks",
    icon: "bi bi-truck h2",
    route: "/trucks",
  },
  {
    label: "Employees",
    icon: "bi bi-people h2",
    route: "/employees",
  },
  {
    label: "Customers",
    icon: "bi bi-building h2",
    route: "/customers",
  },
  {
    label: "Invoices",
    icon: "bi bi-journal-text h2",
    items: [
      {
        label: "Payroll",
        route: "/invoices/payrolls",
      },
      {
        label: "Load",
        route: "/invoices/loads",
      },
    ],
  },
  {
    label: "Subscription",
    icon: "bi bi-credit-card h2",
    route: "/subscription/manage",
  },
] as const;
