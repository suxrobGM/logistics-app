import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { FuelCardProvidersComponent } from "./fuel-card-providers/fuel-card-providers";
import { FuelCardTransactionsComponent } from "./fuel-card-transactions/fuel-card-transactions";

export const fuelCardsRoutes: Routes = [
  {
    path: "",
    component: FuelCardTransactionsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.FuelCard.View,
    },
  },
  {
    path: "providers",
    component: FuelCardProvidersComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Providers",
      permission: Permission.FuelCard.Manage,
    },
  },
];
