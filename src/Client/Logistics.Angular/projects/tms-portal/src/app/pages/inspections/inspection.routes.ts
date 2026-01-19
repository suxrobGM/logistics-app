import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { ConditionReportsListPage } from "./condition-reports-list/condition-reports-list";
import { ConditionReportDetailPage } from "./condition-report-detail/condition-report-detail";

export const inspectionRoutes: Routes = [
  {
    path: "",
    component: ConditionReportsListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Load.View,
    },
  },
  {
    path: ":id",
    component: ConditionReportDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Load.View,
    },
  },
];
