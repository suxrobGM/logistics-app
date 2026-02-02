import type { MenuItem } from "@/shared/layout/panel-menu";

export const sidebarItems: MenuItem[] = [
  {
    label: "Dashboard",
    icon: "pi pi-home text-3xl!",
    items: [
      {
        label: "Home",
        route: "/home",
      },
      {
        label: "Overview",
        route: "/dashboard",
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
    label: "Fleet & Equipment",
    icon: "pi pi-truck text-3xl!",
    items: [
      // Vehicles Section
      {
        label: "Vehicles",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "Trucks",
        route: "/trucks",
      },
      // Maintenance Section
      {
        label: "Maintenance",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "Maintenance Dashboard",
        route: "/maintenance",
      },
      {
        label: "Service Records",
        route: "/maintenance/records",
      },
      {
        label: "Upcoming Service",
        route: "/maintenance/upcoming",
      },
    ],
  },
  {
    label: "Safety & Compliance",
    icon: "pi pi-shield text-3xl!",
    items: [
      {
        label: "Overview",
        route: "/safety",
      },
      // DVIRs Section
      {
        label: "DVIRs",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "DVIR Reports",
        route: "/safety/dvir",
      },
      // Incidents Section
      {
        label: "Incidents",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "Accident Reports",
        route: "/safety/accidents",
      },
      {
        label: "Driver Behavior",
        route: "/safety/driver-behavior",
      },
      {
        label: "Condition Reports",
        route: "/safety/condition-reports",
      },
      // Compliance Section
      {
        label: "Compliance",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "ELD / HOS",
        route: "/eld",
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
    label: "Accounting",
    icon: "pi pi-wallet text-3xl!",
    items: [
      // Payroll Section
      {
        label: "Payroll",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "Payroll Dashboard",
        route: "/payroll",
      },
      {
        label: "Payroll Invoices",
        route: "/payroll/invoices",
      },
      {
        label: "Timesheets",
        route: "/timesheets",
      },
      // Invoicing Section
      {
        label: "Invoicing",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "Load Invoice Dashboard",
        route: "/invoices",
      },
      {
        label: "Load Invoices",
        route: "/invoices/loads",
      },
      // Expenses Section
      {
        label: "Expenses",
        styleClass: "menu-separator",
        disabled: true,
      },
      {
        label: "All Expenses",
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
        label: "Financial Report",
        route: "/reports/financials",
      },
      {
        label: "Payroll Report",
        route: "/reports/payroll",
      },
      {
        label: "Safety Report",
        route: "/reports/safety",
      },
      {
        label: "Maintenance Report",
        route: "/reports/maintenance",
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
        label: "Payments",
        route: "/settings/payments",
      },
      {
        label: "Subscription",
        route: "/subscription/manage",
      },
    ],
  },
] as const;
