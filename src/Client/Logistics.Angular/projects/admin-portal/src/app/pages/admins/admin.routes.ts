import type { Routes } from "@angular/router";
import { AdminsList } from "./admins-list/admins-list";

export const adminRoutes: Routes = [
  {
    path: "",
    component: AdminsList,
    data: {
      breadcrumb: "Admins",
    },
  },
];
