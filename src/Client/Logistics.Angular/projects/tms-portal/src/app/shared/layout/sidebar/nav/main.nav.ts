import type { NavSection } from "@/shared/layout/nav-menu";

export const mainNav: NavSection = {
  id: "main",
  label: "Main",
  items: [
    {
      id: "home",
      label: "Home",
      icon: "house",
      route: "/home",
    },
    {
      id: "messages",
      label: "Messages",
      icon: "messages-square",
      route: "/messages",
      feature: "messages",
      // badge wired in sidebar-nav.service.ts via ChatService
    },
    {
      id: "reports",
      label: "Reports",
      icon: "trending-up",
      route: "/reports",
      feature: "reports",
      // Children stay in the data for the command palette + stored favorites, but each is
      // `menuHidden` so the rendered menu shows Reports as a single link to the hub page.
      children: [
        {
          id: "reports-loads",
          label: "Loads",
          route: "/reports/loads",
          menuHidden: true,
        },
        {
          id: "reports-drivers",
          label: "Drivers",
          route: "/reports/drivers",
          menuHidden: true,
        },
        {
          id: "reports-drivers-detailed",
          label: "Drivers Detailed",
          route: "/reports/drivers/detailed",
          menuHidden: true,
        },
        {
          id: "reports-financial",
          label: "Financial Report",
          route: "/reports/financials",
          menuHidden: true,
        },
        {
          id: "reports-payroll",
          label: "Payroll Report",
          route: "/reports/payroll",
          menuHidden: true,
        },
        {
          id: "reports-safety",
          label: "Safety Report",
          route: "/reports/safety",
          menuHidden: true,
        },
        {
          id: "reports-maintenance",
          label: "Maintenance Report",
          route: "/reports/maintenance",
          menuHidden: true,
        },
        {
          id: "reports-ifta",
          label: "IFTA Report",
          route: "/reports/ifta",
          feature: "ifta",
          menuHidden: true,
        },
        {
          id: "reports-revenue",
          label: "Revenue Overview",
          route: "/reports/revenue",
          menuHidden: true,
        },
        {
          id: "reports-team",
          label: "Team Overview",
          route: "/reports/team",
          menuHidden: true,
        },
      ],
    },
  ],
};
