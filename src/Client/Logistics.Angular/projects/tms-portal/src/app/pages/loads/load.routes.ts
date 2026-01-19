import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { LoadAddComponent } from "./load-add/load-add";
import { LoadDetailPage } from "./load-detail/load-detail";
import { LoadEditComponent } from "./load-edit/load-edit";
import { LoadsListComponent } from "./loads-list/loads-list";

export const loadRoutes: Routes = [
  {
    path: "",
    component: LoadsListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Load.View,
    },
  },
  {
    path: "add",
    component: LoadAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: "import",
    loadComponent: () => import("./load-import/load-import").then((m) => m.LoadImportComponent),
    canActivate: [authGuard],
    data: {
      breadcrumb: "Import",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: ":id/edit",
    component: LoadEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: ":id",
    component: LoadDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Load.View,
    },
  },
];
