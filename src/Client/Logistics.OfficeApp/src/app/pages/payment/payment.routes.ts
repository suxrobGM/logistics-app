import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {ListPaymentsComponent} from "./list-payments/list-payments";

export const paymentRoutes: Routes = [
  {
    path: "",
    component: ListPaymentsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payments",
      permission: Permissions.Payments.View,
    },
  },
];
