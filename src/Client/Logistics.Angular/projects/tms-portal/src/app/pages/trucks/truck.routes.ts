import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { TruckDetailsComponent as TruckDetails } from "./truck-details/truck-details";
import { TruckDocumentsPage } from "./truck-documents/truck-documents";
import { TruckEdit } from "./truck-edit/truck-edit";
import { TrucksList } from "./trucks-list/trucks-list";

export const truckRoutes: Routes = [
  {
    path: "",
    component: TrucksList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Truck.View,
    },
  },
  {
    path: "add",
    component: TruckEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Truck.Manage,
    },
  },
  {
    path: ":id/edit",
    component: TruckEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Truck.Manage,
    },
  },
  {
    path: ":id/documents",
    component: TruckDocumentsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Documents",
      permission: Permission.Truck.Manage,
    },
  },
  {
    path: ":id",
    component: TruckDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Truck.View,
    },
  },
];
