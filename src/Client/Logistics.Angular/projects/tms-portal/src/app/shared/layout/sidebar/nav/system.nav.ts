import { Permission } from "@logistics/shared";
import type { NavSection } from "@/shared/layout/nav-menu";

export const systemNav: NavSection = {
  id: "system",
  label: "System",
  pinToBottom: true,
  items: [
    {
      id: "settings",
      label: "Settings",
      icon: "settings",
      route: "/settings",
      permission: Permission.Tenant.View,
      children: [
        {
          id: "settings-company",
          label: "Company",
          route: "/settings/company",
          permission: Permission.Tenant.View,
          menuHidden: true,
        },
        {
          id: "settings-billing",
          label: "Billing",
          route: "/settings/billing",
          permission: Permission.Payment.View,
          menuHidden: true,
        },
        {
          id: "settings-integrations",
          label: "Integrations",
          route: "/settings/integrations",
          feature: ["accounting", "mcp_server"],
          permission: Permission.Accounting.View,
          menuHidden: true,
        },
        {
          id: "settings-features",
          label: "Features",
          route: "/settings/features",
          permission: Permission.Tenant.View,
          menuHidden: true,
        },
      ],
    },
  ],
};
