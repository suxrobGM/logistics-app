import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { AccidentAddPage } from "./accident-add/accident-add";
import { AccidentDetailPage } from "./accident-detail/accident-detail";
import { AccidentEditPage } from "./accident-edit/accident-edit";
import { AccidentsListPage } from "./accidents-list/accidents-list";
import { ConditionReportDetailPage } from "./condition-report-detail/condition-report-detail";
import { ConditionReportsListPage } from "./condition-reports-list/condition-reports-list";
import { DriverBehaviorListPage } from "./driver-behavior-list/driver-behavior-list";
import { DvirDetailPage } from "./dvir-detail/dvir-detail";
import { DvirListPage } from "./dvir-list/dvir-list";
import { DvirReviewPage } from "./dvir-review/dvir-review";
import { InspectionsDashboardPage } from "./inspections-dashboard/inspections-dashboard";

export const inspectionRoutes: Routes = [
  {
    path: "",
    component: InspectionsDashboardPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Safety.View,
    },
  },
  // Condition Reports
  {
    path: "condition-reports",
    component: ConditionReportsListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Condition Reports",
      permission: Permission.Load.View,
    },
  },
  {
    path: "condition-reports/:id",
    component: ConditionReportDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Load.View,
    },
  },
  // DVIR
  {
    path: "dvir",
    component: DvirListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "DVIR Reports",
      permission: Permission.Safety.View,
    },
  },
  {
    path: "dvir/:id",
    component: DvirDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "DVIR Details",
      permission: Permission.Safety.View,
    },
  },
  {
    path: "dvir/:id/review",
    component: DvirReviewPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Review DVIR",
      permission: Permission.Safety.Manage,
    },
  },
  // Accidents
  {
    path: "accidents",
    component: AccidentsListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Accident Reports",
      permission: Permission.Safety.View,
    },
  },
  {
    path: "accidents/add",
    component: AccidentAddPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Report Accident",
      permission: Permission.Safety.Manage,
    },
  },
  {
    path: "accidents/:id",
    component: AccidentDetailPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Accident Details",
      permission: Permission.Safety.View,
    },
  },
  {
    path: "accidents/:id/edit",
    component: AccidentEditPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit Accident",
      permission: Permission.Safety.Manage,
    },
  },
  // Driver Behavior
  {
    path: "driver-behavior",
    component: DriverBehaviorListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Driver Behavior",
      permission: Permission.Safety.View,
    },
  },
];
