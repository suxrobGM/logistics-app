import type { Routes } from "@angular/router";
import { authGuard } from "@/core/auth";
import { Permission } from "@/shared/models";
import { EldDashboardComponent } from "./eld-dashboard/eld-dashboard";
import { EldDriverMappingsComponent } from "./eld-driver-mappings/eld-driver-mappings";
import { EldHosLogsComponent } from "./eld-hos-logs/eld-hos-logs";
import { EldProvidersComponent } from "./eld-providers/eld-providers";

export const eldRoutes: Routes = [
  {
    path: "",
    component: EldDashboardComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Eld.View,
    },
  },
  {
    path: "providers",
    component: EldProvidersComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Providers",
      permission: Permission.Eld.Manage,
    },
  },
  {
    path: "providers/:providerId/mappings",
    component: EldDriverMappingsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Driver Mappings",
      permission: Permission.Eld.Manage,
    },
  },
  {
    path: "drivers/:employeeId/logs",
    component: EldHosLogsComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "HOS Logs",
      permission: Permission.Eld.View,
    },
  },
];
