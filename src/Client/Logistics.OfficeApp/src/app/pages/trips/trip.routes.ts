import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {TripDetails} from "./trip-details/trip-details";
import {TripEdit} from "./trip-edit/trip-edit";
import {TripsList} from "./trips-list/trips-list";

export const tripRoutes: Routes = [
  {
    path: "",
    component: TripsList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Trips",
    },
  },
  {
    path: "add",
    component: TripEdit,
    data: {
      breadcrumb: "Add Trip",
    },
  },
  {
    path: ":tripId/edit",
    component: TripEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Trip",
    },
  },
  {
    path: ":tripId",
    component: TripDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Trip Details",
    },
  },
];
