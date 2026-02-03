import type { MenuItem } from "@/shared/layout/panel-menu";

export const sidebarItems: MenuItem[] = [
  {
    label: "Home",
    icon: "pi pi-home",
    route: "/home",
  },
  {
    label: "Dashboard",
    icon: "pi pi-chart-bar",
    route: "/dashboard",
    feature: "dashboard",
  },
  {
    label: "Messages",
    icon: "pi pi-comments",
    route: "/messages",
    feature: "messages",
  },
  {
    label: "Operations",
    icon: "pi pi-server",
    items: [
      {
        label: "Loads",
        route: "/loads",
        feature: "loads",
      },
      {
        label: "Trips",
        route: "/trips",
        feature: "trips",
      },
      {
        label: "Load Board",
        route: "/loadboard",
        feature: "loadboard",
      },
      {
        label: "Search Loads",
        route: "/loadboard/search",
        feature: "loadboard",
      },
      {
        label: "Posted Trucks",
        route: "/loadboard/posted-trucks",
        feature: "loadboard",
      },
      {
        label: "Providers",
        route: "/loadboard/providers",
        feature: "loadboard",
      },
    ],
  },
  {
    label: "Fleet",
    icon: "pi pi-truck",
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
        feature: "trucks",
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
        feature: "maintenance",
      },
      {
        label: "Service Records",
        route: "/maintenance/records",
        feature: "maintenance",
      },
      {
        label: "Upcoming Service",
        route: "/maintenance/upcoming",
        feature: "maintenance",
      },
    ],
  },
  {
    label: "Safety",
    icon: "pi pi-shield",
    items: [
      {
        label: "Overview",
        route: "/safety",
        feature: "safety",
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
        feature: "safety",
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
        feature: "safety",
      },
      {
        label: "Driver Behavior",
        route: "/safety/driver-behavior",
        feature: "safety",
      },
      {
        label: "Condition Reports",
        route: "/safety/condition-reports",
        feature: "safety",
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
        feature: "eld",
      },
    ],
  },
  {
    label: "Directory",
    icon: "pi pi-users",
    items: [
      {
        label: "Employees",
        route: "/employees",
        feature: "employees",
      },
      {
        label: "Customers",
        route: "/customers",
        feature: "customers",
      },
    ],
  },
  {
    label: "Accounting",
    icon: "pi pi-wallet",
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
        feature: "payroll",
      },
      {
        label: "Payroll Invoices",
        route: "/payroll/invoices",
        feature: "payroll",
      },
      {
        label: "Timesheets",
        route: "/timesheets",
        feature: "timesheets",
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
        feature: "invoices",
      },
      {
        label: "Load Invoices",
        route: "/invoices/loads",
        feature: "invoices",
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
        feature: "expenses",
      },
      {
        label: "Add Expense",
        route: "/expenses/add",
        feature: "expenses",
      },
      {
        label: "Expense Analytics",
        route: "/expenses/analytics",
        feature: "expenses",
      },
    ],
  },
  {
    label: "Reports",
    icon: "pi pi-chart-line",
    feature: "reports",
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
    icon: "pi pi-cog",
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
        label: "Features",
        route: "/settings/features",
      },
      {
        label: "Subscription",
        route: "/subscription/manage",
      },
    ],
  },
];
