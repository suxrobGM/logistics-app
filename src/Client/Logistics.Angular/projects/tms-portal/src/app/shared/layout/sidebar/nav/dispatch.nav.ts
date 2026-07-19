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
    },
    {
      id: "trips",
      label: "Trips",
      icon: "map",
      route: "/trips",
      feature: "trips",
    },
    {
      id: "ai-dispatch",
      label: "AI Dispatch",
      icon: "sparkles",
      route: "/ai-dispatch",
      feature: "agentic_dispatch",
    },
    {
      id: "loadboard",
      label: "Load Board",
      icon: "search",
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
      icon: "warehouse",
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
};
