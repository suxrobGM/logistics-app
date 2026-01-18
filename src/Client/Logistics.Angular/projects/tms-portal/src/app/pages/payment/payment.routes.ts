import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
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
