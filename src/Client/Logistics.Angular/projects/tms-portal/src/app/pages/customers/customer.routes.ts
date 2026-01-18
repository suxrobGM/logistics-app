import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { CustomerAddComponent } from "./customer-add/customer-add";
import { CustomerEditComponent } from "./customer-edit/customer-edit";
import { CustomersListComponent } from "./customers-list/customers-list";

export const customerRoutes: Routes = [
  {
    path: "",
    component: CustomersListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Customer.View,
    },
  },
  {
    path: "add",
    component: CustomerAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Customer.Manage,
    },
  },
  {
    path: ":id/edit",
    component: CustomerEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Customer.Manage,
    },
  },
];
