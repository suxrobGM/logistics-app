import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permission } from "@/shared/models";
import { PaymentsListComponent } from "./payments-list/payments-list";

export const paymentRoutes: Routes = [
  {
    path: "",
    component: PaymentsListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Payments",
      permission: Permission.Payment.View,
    },
  },
];
