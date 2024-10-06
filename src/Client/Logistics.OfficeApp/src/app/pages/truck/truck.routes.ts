import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {authGuard} from "@/core/guards";
import {EditTruckComponent} from "./edit-truck/edit-truck.component";
import {ListTruckComponent} from "./list-trucks/list-trucks.component";
import {TruckDetailsComponent} from "./truck-details/truck-details.component";

export const TRUCK_ROUTES: Routes = [
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
