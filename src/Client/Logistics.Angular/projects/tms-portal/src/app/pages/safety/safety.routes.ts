import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { SafetyDashboardPage } from "./safety-dashboard/safety-dashboard";
import { DvirListPage } from "./dvir-list/dvir-list";
import { DvirDetailPage } from "./dvir-detail/dvir-detail";
import { DvirReviewPage } from "./dvir-review/dvir-review";
import { AccidentsListPage } from "./accidents-list/accidents-list";
import { AccidentAddPage } from "./accident-add/accident-add";
import { AccidentDetailPage } from "./accident-detail/accident-detail";
import { DriverBehaviorListPage } from "./driver-behavior-list/driver-behavior-list";

export const safetyRoutes: Routes = [
  {
    path: "",
    component: SafetyDashboardPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Safety.View,
    },
  },
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
    path: "behavior",
    component: DriverBehaviorListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Driver Behavior",
      permission: Permission.Safety.View,
    },
  },
];
