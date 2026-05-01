import type { NavSection } from "@/shared/layout/nav-menu";

export const sidebarSections: NavSection[] = [
  {
    id: "main",
    label: "Main",
    items: [
      {
        id: "home",
        label: "Home",
        icon: "pi pi-home",
        route: "/home",
      },
      {
        id: "messages",
        label: "Messages",
        icon: "pi pi-comments",
        route: "/messages",
        feature: "messages",
        // badge wired in sidebar.ts via ChatService
      },
    ],
  },
  {
    id: "dispatch",
    label: "Dispatch",
    items: [
      {
        id: "loads",
        label: "Loads",
        icon: "pi pi-box",
        route: "/loads",
        feature: "loads",
      },
      {
        id: "trips",
        label: "Trips",
        icon: "pi pi-map",
        route: "/trips",
        feature: "trips",
      },
      {
        id: "ai-dispatch",
        label: "AI Dispatch",
        icon: "pi pi-sparkles",
        route: "/ai-dispatch",
        feature: "agentic_dispatch",
      },
      {
        id: "loadboard",
        label: "Load Board",
        icon: "pi pi-search",
        feature: "load_board",
        children: [
          {
            id: "loadboard-search",
            label: "Search Loads",
            route: "/loadboard/search",
          },
          {
            id: "loadboard-posted-trucks",
            label: "Posted Trucks",
            route: "/loadboard/posted-trucks",
          },
          {
            id: "loadboard-providers",
            label: "Providers",
            route: "/loadboard/providers",
          },
        ],
      },
      {
        id: "intermodal",
        label: "Intermodal",
        icon: "pi pi-warehouse",
        children: [
          {
            id: "intermodal-containers",
            label: "Containers",
            route: "/containers",
          },
          {
            id: "intermodal-terminals",
            label: "Terminals",
            route: "/terminals",
          },
        ],
      },
    ],
  },
  {
    id: "fleet",
    label: "Fleet & Compliance",
    items: [
      {
        id: "trucks",
        label: "Trucks",
        icon: "pi pi-truck",
        route: "/trucks",
        feature: "trucks",
      },
      {
        id: "eld",
        label: "ELD / HOS",
        icon: "pi pi-clock",
        route: "/eld",
        feature: "eld",
      },
      {
        id: "maintenance",
        label: "Maintenance",
        icon: "pi pi-wrench",
        feature: "maintenance",
        children: [
          {
            id: "maintenance-dashboard",
            label: "Dashboard",
            route: "/maintenance",
          },
          {
            id: "maintenance-records",
            label: "Service Records",
            route: "/maintenance/records",
          },
          {
            id: "maintenance-upcoming",
            label: "Upcoming Service",
            route: "/maintenance/upcoming",
          },
        ],
      },
      {
        id: "safety",
        label: "Safety",
        icon: "pi pi-shield",
        feature: "safety",
        children: [
          {
            id: "safety-overview",
            label: "Overview",
            route: "/safety",
          },
          {
            id: "safety-dvir",
            label: "DVIR Reports",
            route: "/safety/dvir",
          },
          {
            id: "safety-accidents",
            label: "Accidents",
            route: "/safety/accidents",
          },
          {
            id: "safety-driver-behavior",
            label: "Driver Behavior",
            route: "/safety/driver-behavior",
          },
          {
            id: "safety-condition-reports",
            label: "Condition Reports",
            route: "/safety/condition-reports",
          },
        ],
      },
    ],
  },
  {
    id: "business",
    label: "Business",
    items: [
      {
        id: "employees",
        label: "Employees",
        icon: "pi pi-users",
        route: "/employees",
        feature: "employees",
      },
      {
        id: "customers",
        label: "Customers",
        icon: "pi pi-building",
        route: "/customers",
        feature: "customers",
      },
      {
        id: "payroll",
        label: "Payroll",
        icon: "pi pi-wallet",
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
        icon: "pi pi-file-edit",
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
        icon: "pi pi-money-bill",
        feature: "expenses",
        children: [
          {
            id: "expenses-all",
            label: "All Expenses",
            route: "/expenses",
          },
          {
            id: "expenses-add",
            label: "Add Expense",
            route: "/expenses/add",
          },
          {
            id: "expenses-analytics",
            label: "Analytics",
            route: "/expenses/analytics",
          },
        ],
      },
      {
        id: "reports",
        label: "Reports",
        icon: "pi pi-chart-line",
        feature: "reports",
        children: [
          {
            id: "reports-loads",
            label: "Loads",
            route: "/reports/loads",
          },
          {
            id: "reports-drivers",
            label: "Drivers",
            route: "/reports/drivers",
          },
          {
            id: "reports-drivers-detailed",
            label: "Drivers Detailed",
            route: "/reports/drivers/detailed",
          },
          {
            id: "reports-financial",
            label: "Financial Report",
            route: "/reports/financials",
          },
          {
            id: "reports-payroll",
            label: "Payroll Report",
            route: "/reports/payroll",
          },
          {
            id: "reports-safety",
            label: "Safety Report",
            route: "/reports/safety",
          },
          {
            id: "reports-maintenance",
            label: "Maintenance Report",
            route: "/reports/maintenance",
          },
          {
            id: "reports-revenue",
            label: "Revenue Overview",
            route: "/reports/revenue",
          },
          {
            id: "reports-team",
            label: "Team Overview",
            route: "/reports/team",
          },
        ],
      },
    ],
  },
  {
    id: "system",
    label: "System",
    pinToBottom: true,
    items: [
      {
        id: "settings",
        label: "Settings",
        icon: "pi pi-cog",
        children: [
          {
            id: "settings-company",
            label: "Company",
            route: "/settings/company",
          },
          {
            id: "settings-payments",
            label: "Payments",
            route: "/settings/payments",
          },
          {
            id: "settings-features",
            label: "Features",
            route: "/settings/features",
          },
          {
            id: "settings-ai",
            label: "AI Settings",
            route: "/settings/ai",
          },
          {
            id: "settings-api-keys",
            label: "API Keys",
            route: "/settings/api-keys",
            feature: "mcp_server",
          },
          {
            id: "settings-subscription",
            label: "Subscription",
            route: "/subscription/manage",
          },
        ],
      },
    ],
  },
];
