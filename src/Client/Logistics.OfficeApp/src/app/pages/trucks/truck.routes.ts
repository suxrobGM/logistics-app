import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {EditTruckComponent} from "./edit-truck/edit-truck";
import {ListTruckComponent} from "./list-trucks/list-trucks";
import {TruckDetailsComponent} from "./truck-details/truck-details";

export const truckRoutes: Routes = [
  {
    path: "",
    component: ListTruckComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Trucks.View,
    },
  },
  {
    path: "add",
    component: EditTruckComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Trucks.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditTruckComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Trucks.Edit,
    },
  },
  {
    path: "view/:id",
    component: TruckDetailsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permissions.Trucks.View,
    },
  },
];
