import type { NavSection } from "@/shared/layout/nav-menu";

export const fleetNav: NavSection = {
  id: "fleet",
  label: "Fleet & Compliance",
  items: [
    {
      id: "trucks",
      label: "Trucks",
      icon: "truck",
      route: "/trucks",
      feature: "trucks",
    },
    {
      id: "eld",
      label: "ELD / HOS",
      icon: "clock",
      route: "/eld",
      feature: "eld",
    },
    {
      id: "maintenance",
      label: "Maintenance",
      icon: "wrench",
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
      icon: "shield",
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
};
