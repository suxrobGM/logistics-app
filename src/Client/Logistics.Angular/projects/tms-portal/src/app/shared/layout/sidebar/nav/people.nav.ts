import type { NavSection } from "@/shared/layout/nav-menu";

export const peopleNav: NavSection = {
  id: "people",
  label: "People & Partners",
  items: [
    {
      id: "employees",
      label: "Employees",
      icon: "users",
      route: "/employees",
      feature: "employees",
    },
    {
      id: "customers",
      label: "Customers",
      icon: "building-2",
      route: "/customers",
      feature: "customers",
    },
  ],
};
