import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {AuthGuard} from "@/core/guards";
import {ListCustomersComponent} from "./list-customers/list-customers.component";
import {EditCustomerComponent} from "./edit-customer/edit-customer.component";

export const CUSTOMER_ROUTES: Routes = [
  {
    path: "",
    component: ListCustomersComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Customers.View,
    },
  },
  {
    path: "add",
    component: EditCustomerComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Customers.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditCustomerComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Customers.Edit,
    },
  },
];
