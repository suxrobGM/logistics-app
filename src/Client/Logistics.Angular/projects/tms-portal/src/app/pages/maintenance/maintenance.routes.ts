import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { MaintenanceDashboardPage } from "./maintenance-dashboard/maintenance-dashboard";
import { ServiceRecordsPage } from "./service-records/service-records";
import { ServiceRecordAddPage } from "./service-record-add/service-record-add";
import { ServiceRecordDetailPage } from "./service-record-detail/service-record-detail";
import { ServiceRecordEditPage } from "./service-record-edit/service-record-edit";
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
    path: "records/add",
    component: ServiceRecordAddPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Log Service Record",
      permission: Permission.Maintenance.Manage,
    },
  },
  {
    path: "records/:id",
    component: ServiceRecordDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Service Record Details",
      permission: Permission.Maintenance.View,
    },
  },
  {
    path: "records/:id/edit",
    component: ServiceRecordEditPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Service Record",
      permission: Permission.Maintenance.Manage,
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
