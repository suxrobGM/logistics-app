import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {TripAddPage} from "./trip-add/trip-add";
import {TripDetailsPage} from "./trip-details/trip-details";
import {TripEditPage} from "./trip-edit/trip-edit";
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
    component: TripAddPage,
    data: {
      breadcrumb: "Add Trip",
    },
  },
  {
    path: ":tripId/edit",
    component: TripEditPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Trip",
    },
  },
  {
    path: ":tripId",
    component: TripDetailsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Trip Details",
    },
  },
];
