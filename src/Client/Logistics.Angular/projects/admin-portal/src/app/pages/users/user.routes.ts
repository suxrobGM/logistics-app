import type { Routes } from "@angular/router";
import { UsersList } from "./users-list/users-list";

export const userRoutes: Routes = [
  {
    path: "",
    component: UsersList,
    data: {
      breadcrumb: "Users",
    },
  },
];
