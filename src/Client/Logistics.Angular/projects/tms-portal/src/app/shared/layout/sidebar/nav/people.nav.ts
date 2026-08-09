import { Permission } from "@logistics/shared";
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
      // Not `Employee.View` - that is in the basic set every role holds, drivers included.
      permission: Permission.Employee.Manage,
    },
    {
      id: "customers",
      label: "Customers",
      icon: "building-2",
      route: "/customers",
      feature: "customers",
      permission: Permission.Customer.View,
    },
  ],
};
