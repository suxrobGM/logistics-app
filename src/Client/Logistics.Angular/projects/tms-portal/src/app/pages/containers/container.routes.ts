import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { ContainerAdd } from "./container-add/container-add";
import { ContainerDetails } from "./container-details/container-details";
import { ContainerEdit } from "./container-edit/container-edit";
import { ContainersList } from "./containers-list/containers-list";

export const containerRoutes: Routes = [
  {
    path: "",
    component: ContainersList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Container.View,
    },
  },
  {
    path: "add",
    component: ContainerAdd,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Container.Manage,
    },
  },
  {
    path: ":id/edit",
    component: ContainerEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Container.Manage,
    },
  },
  {
    path: ":id",
    component: ContainerDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Container.View,
    },
  },
];
