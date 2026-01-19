import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { LoadAddComponent } from "./load-add/load-add";
import { LoadDocumentsPage } from "./load-documents/load-documents";
import { LoadEditComponent } from "./load-edit/load-edit";
import { LoadPodViewerPage } from "./load-pod-viewer/load-pod-viewer";
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
    path: ":id/documents",
    component: LoadDocumentsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Documents",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: ":id/pod",
    component: LoadPodViewerPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "POD/BOL",
      permission: Permission.Load.View,
    },
  },
];
