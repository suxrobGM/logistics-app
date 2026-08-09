import { Permission } from "@logistics/shared";
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
      permission: Permission.Message.View,
    },
    {
      id: "reports",
      label: "Reports",
      icon: "trending-up",
      route: "/reports",
      feature: "reports",
      permission: Permission.Stat.View,
      children: [
        {
          id: "reports-loads",
          label: "Loads",
          route: "/reports/loads",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-drivers",
          label: "Drivers",
          route: "/reports/drivers",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-drivers-detailed",
          label: "Drivers Detailed",
          route: "/reports/drivers/detailed",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-financial",
          label: "Financial Report",
          route: "/reports/financials",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-payroll",
          label: "Payroll Report",
          route: "/reports/payroll",
          permission: Permission.Payroll.View,
          menuHidden: true,
        },
        {
          id: "reports-safety",
          label: "Safety Report",
          route: "/reports/safety",
          permission: Permission.Safety.View,
          menuHidden: true,
        },
        {
          id: "reports-maintenance",
          label: "Maintenance Report",
          route: "/reports/maintenance",
          permission: Permission.Truck.View,
          menuHidden: true,
        },
        {
          id: "reports-ifta",
          label: "IFTA Report",
          route: "/reports/ifta",
          feature: "ifta",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-revenue",
          label: "Revenue Overview",
          route: "/reports/revenue",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
        {
          id: "reports-team",
          label: "Team Overview",
          route: "/reports/team",
          permission: Permission.Stat.View,
          menuHidden: true,
        },
      ],
    },
  ],
};
