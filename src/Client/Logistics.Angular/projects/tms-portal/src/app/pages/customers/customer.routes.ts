import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { CustomerAddComponent } from "./customer-add/customer-add";
import { CustomerDetails } from "./customer-details/customer-details";
import { CustomersList } from "./customers-list/customers-list";

export const customerRoutes: Routes = [
  {
    path: "",
    component: CustomersList,
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
    path: ":id",
    component: CustomerDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Customer.View,
    },
  },
];
