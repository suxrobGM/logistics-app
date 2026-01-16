import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared/models";
import { authGuard } from "@/core/auth";
import { TruckDetailsComponent } from "./truck-details/truck-details";
import { TruckEditComponent } from "./truck-edit/truck-edit";
import { TrucksListComponent } from "./trucks-list/trucks-list";

export const truckRoutes: Routes = [
  {
    path: "",
    component: TrucksListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Truck.View,
    },
  },
  {
    path: "add",
    component: TruckEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Truck.Manage,
    },
  },
  {
    path: ":id/edit",
    component: TruckEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Truck.Manage,
    },
  },
  {
    path: ":id",
    component: TruckDetailsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Truck.View,
    },
  },
];
