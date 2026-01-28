import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { NotificationsComponent } from "./notifications";

export const notificationsRoutes: Routes = [
  {
    path: "",
    component: NotificationsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Notifications",
    },
  },
];
