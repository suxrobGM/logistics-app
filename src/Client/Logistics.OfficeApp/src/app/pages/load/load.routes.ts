import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {AddLoadComponent} from "./add-load/add-load.component";
import {EditLoadComponent} from "./edit-load/edit-load.component";
import {ListLoadComponent} from "./list-loads/list-loads.component";

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
    path: "edit/:id",
    component: EditLoadComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Loads.Edit,
    },
  },
];
