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
      // Children stay in the data for the command palette + stored favorites, but each is
      // `menuHidden` so the rendered menu shows Settings as a single link to the tabbed layout.
      children: [
        {
          id: "settings-company",
          label: "Company",
          route: "/settings/company",
          menuHidden: true,
        },
        {
          id: "settings-payments",
          label: "Payments",
          route: "/settings/payments",
          menuHidden: true,
        },
        {
          id: "settings-subscription",
          label: "Plan & Billing",
          route: "/subscription/manage",
          menuHidden: true,
        },
        {
          id: "settings-features",
          label: "Features",
          route: "/settings/features",
          menuHidden: true,
        },
        {
          id: "settings-api-keys",
          label: "API Keys",
          route: "/settings/api-keys",
          feature: "mcp_server",
          menuHidden: true,
        },
        {
          id: "settings-accounting",
          label: "Accounting",
          route: "/settings/accounting",
          feature: "accounting",
          menuHidden: true,
        },
        {
          id: "settings-privacy",
          label: "Privacy",
          route: "/settings/privacy",
          menuHidden: true,
        },
      ],
    },
  ],
};
