import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {AddLoadComponent} from "./add-load/add-load";
import {EditLoadComponent} from "./edit-load/edit-load";
import {ListLoadComponent} from "./list-loads/list-loads";

export const loadRoutes: Routes = [
  {
    path: "",
    component: ListLoadComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Loads.View,
    },
  },
  {
    path: "add",
    component: AddLoadComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Loads.Create,
    },
  },
  {
    path: ":id/edit",
    component: EditLoadComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Loads.Edit,
    },
  },
];
