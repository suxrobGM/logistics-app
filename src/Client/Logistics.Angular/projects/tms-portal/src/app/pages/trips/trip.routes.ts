import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { TripAddPage } from "./trip-add/trip-add";
import { TripDetailsPage } from "./trip-details/trip-details";
import { TripEditPage } from "./trip-edit/trip-edit";
import { TripsList } from "./trips-list/trips-list";

export const tripRoutes: Routes = [
  {
    path: "",
    component: TripsList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Trips",
      permission: Permission.Load.View,
    },
  },
  {
    path: "add",
    component: TripAddPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add Trip",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: ":tripId/edit",
    component: TripEditPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Trip",
      permission: Permission.Load.Manage,
    },
  },
  {
    path: ":tripId",
    component: TripDetailsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Trip Details",
      permission: Permission.Load.View,
    },
  },
];
