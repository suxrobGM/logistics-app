import type { Routes } from "@angular/router";
import { featureGuardFromData, Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { AccidentAddPage } from "./accident-add/accident-add";
import { AccidentDetailsPage } from "./accident-details/accident-details";
import { AccidentEditPage } from "./accident-edit/accident-edit";
import { AccidentsListPage } from "./accidents-list/accidents-list";
import { ConditionReportDetailsPage } from "./condition-report-details/condition-report-details";
import { ConditionReportsListPage } from "./condition-reports-list/condition-reports-list";
import { DriverBehaviorListPage } from "./driver-behavior-list/driver-behavior-list";
import { DvirDetailsPage } from "./dvir-details/dvir-details";
import { DvirListPage } from "./dvir-list/dvir-list";
import { DvirReviewPage } from "./dvir-review/dvir-review";
import { InspectionsDashboardPage } from "./inspections-dashboard/inspections-dashboard";

export const inspectionRoutes: Routes = [
  {
    path: "",
    component: InspectionsDashboardPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "",
      permission: Permission.Safety.View,
      feature: "safety",
    },
  },
  // Condition Reports
  {
    path: "condition-reports",
    component: ConditionReportsListPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Condition Reports",
      permission: Permission.Load.View,
      feature: "safety",
    },
  },
  {
    path: "condition-reports/:id",
    component: ConditionReportDetailsPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Details",
      permission: Permission.Load.View,
      feature: "safety",
    },
  },
  // DVIR
  {
    path: "dvir",
    component: DvirListPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "DVIR Reports",
      permission: Permission.Dvir.View,
      feature: "dvir",
    },
  },
  {
    path: "dvir/:id",
    component: DvirDetailsPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "DVIR Details",
      permission: Permission.Dvir.View,
      feature: "dvir",
    },
  },
  {
    path: "dvir/:id/review",
    component: DvirReviewPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Review DVIR",
      permission: Permission.Dvir.Review,
      feature: "dvir",
    },
  },
  // Accidents
  {
    path: "accidents",
    component: AccidentsListPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Accident Reports",
      permission: Permission.Safety.View,
      feature: "safety",
    },
  },
  {
    path: "accidents/add",
    component: AccidentAddPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Report Accident",
      permission: Permission.Safety.Manage,
      feature: "safety",
    },
  },
  {
    path: "accidents/:id",
    component: AccidentDetailsPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Accident Details",
      permission: Permission.Safety.View,
      feature: "safety",
    },
  },
  {
    path: "accidents/:id/edit",
    component: AccidentEditPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Edit Accident",
      permission: Permission.Safety.Manage,
      feature: "safety",
    },
  },
  // Driver Behavior
  {
    path: "driver-behavior",
    component: DriverBehaviorListPage,
    canActivate: [authGuard, featureGuardFromData],
    data: {
      breadcrumb: "Driver Behavior",
      permission: Permission.Safety.View,
      feature: "safety",
    },
  },
];
