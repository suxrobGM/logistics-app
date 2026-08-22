import { Permission } from "@logistics/shared";
import type { NavSection } from "@/shared/layout/nav-menu";

export const dispatchNav: NavSection = {
  id: "dispatch",
  label: "Dispatch",
  items: [
    {
      id: "loads",
      label: "Loads",
      icon: "package",
      route: "/loads",
      feature: "loads",
      permission: Permission.Load.View,
    },
    {
      id: "trips",
      label: "Trips",
      icon: "map",
      route: "/trips",
      feature: "trips",
      permission: Permission.Load.View,
    },
    {
      id: "ai-dispatch",
      label: "AI Dispatch",
      icon: "sparkles",
      route: "/ai-dispatch",
      feature: "agentic_dispatch",
      permission: Permission.Dispatch.View,
    },
    {
      id: "negotiations",
      label: "Negotiations",
      icon: "mail",
      route: "/ai-dispatch/negotiations",
      feature: "ai_rate_negotiation",
      permission: Permission.Negotiation.View,
    },
    {
      id: "rate-floors",
      label: "Rate Floors",
      icon: "chart-column",
      route: "/ai-dispatch/rate-floors",
      feature: "ai_rate_negotiation",
      permission: Permission.Negotiation.View,
    },
    {
      id: "loadboard",
      label: "Load Board",
      icon: "search",
      feature: "load_board",
      permission: Permission.LoadBoard.View,
      children: [
        {
          id: "loadboard-search",
          label: "Search Loads",
          route: "/loadboard/search",
          permission: Permission.LoadBoard.Search,
        },
        {
          id: "loadboard-posted-trucks",
          label: "Posted Trucks",
          route: "/loadboard/posted-trucks",
          permission: Permission.LoadBoard.Post,
        },
        {
          id: "loadboard-providers",
          label: "Providers",
          route: "/loadboard/providers",
          permission: Permission.LoadBoard.Manage,
        },
      ],
    },
    {
      id: "intermodal",
      label: "Intermodal",
      icon: "warehouse",
      feature: "intermodal_containers",
      permission: Permission.Container.View,
      children: [
        {
          id: "intermodal-containers",
          label: "Containers",
          route: "/containers",
          permission: Permission.Container.View,
        },
        {
          id: "intermodal-terminals",
          label: "Terminals",
          route: "/terminals",
          permission: Permission.Terminal.View,
        },
      ],
    },
  ],
};
