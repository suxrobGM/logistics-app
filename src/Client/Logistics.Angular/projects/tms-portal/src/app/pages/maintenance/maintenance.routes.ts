import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { MaintenanceDashboardPage } from "./maintenance-dashboard/maintenance-dashboard";
import { ServiceRecordsPage } from "./service-records/service-records";
import { UpcomingServicePage } from "./upcoming-service/upcoming-service";

export const maintenanceRoutes: Routes = [
  {
    path: "",
    component: MaintenanceDashboardPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Maintenance.View,
    },
  },
  {
    path: "records",
    component: ServiceRecordsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Service Records",
      permission: Permission.Maintenance.View,
    },
  },
  {
    path: "upcoming",
    component: UpcomingServicePage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Upcoming Service",
      permission: Permission.Maintenance.View,
    },
  },
];
