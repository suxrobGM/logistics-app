import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permissions } from "@/shared/models";
import { LoadAddComponent } from "./load-add/load-add";
import { LoadDocumentsPage } from "./load-documents/load-documents";
import { LoadEditComponent } from "./load-edit/load-edit";
import { LoadsListComponent } from "./loads-list/loads-list";

export const loadRoutes: Routes = [
  {
    path: "",
    component: LoadsListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Loads.View,
    },
  },
  {
    path: "add",
    component: LoadAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Loads.Create,
    },
  },
  {
    path: ":id/edit",
    component: LoadEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Loads.Edit,
    },
  },
  {
    path: ":id/documents",
    component: LoadDocumentsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Documents",
      permission: Permissions.Loads.Edit,
    },
  },
];
