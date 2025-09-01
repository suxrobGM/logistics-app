import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {TruckDetailsComponent} from "./truck-details/truck-details";
import {TruckEditComponent} from "./truck-edit/truck-edit";
import {TrucksListComponent} from "./trucks-list/trucks-list";

export const truckRoutes: Routes = [
  {
    path: "",
    component: TrucksListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Trucks.View,
    },
  },
  {
    path: "add",
    component: TruckEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Trucks.Create,
    },
  },
  {
    path: ":id/edit",
    component: TruckEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Trucks.Edit,
    },
  },
  {
    path: ":id",
    component: TruckDetailsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permissions.Trucks.View,
    },
  },
];
