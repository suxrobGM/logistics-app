import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {authGuard} from "@/core/guards";
import {ListCustomersComponent} from "./list-customers/list-customers.component";
import {EditCustomerComponent} from "./edit-customer/edit-customer.component";

export const CUSTOMER_ROUTES: Routes = [
  {
    path: "",
    component: ListCustomersComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Customers.View,
    },
  },
  {
    path: "add",
    component: EditCustomerComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Customers.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditCustomerComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Customers.Edit,
    },
  },
];
