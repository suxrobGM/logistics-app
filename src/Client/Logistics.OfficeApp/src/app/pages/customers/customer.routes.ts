import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {CustomerAddComponent} from "./customer-add/customer-add";
import {CustomerEditComponent} from "./customer-edit/customer-edit";
import {CustomersListComponent} from "./customers-list/customers-list";

export const customerRoutes: Routes = [
  {
    path: "",
    component: CustomersListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Customers.View,
    },
  },
  {
    path: "add",
    component: CustomerAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Customers.Create,
    },
  },
  {
    path: ":id/edit",
    component: CustomerEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Customers.Edit,
    },
  },
];
