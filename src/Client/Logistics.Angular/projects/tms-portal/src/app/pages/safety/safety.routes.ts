import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { DvirListPage } from "./dvir-list/dvir-list";
import { AccidentsListPage } from "./accidents-list/accidents-list";
import { EmergencyContactsPage } from "./emergency-contacts/emergency-contacts";

export const safetyRoutes: Routes = [
  {
    path: "",
    redirectTo: "dvir",
    pathMatch: "full",
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
    path: "accidents",
    component: AccidentsListPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Accident Reports",
      permission: Permission.Safety.View,
    },
  },
  {
    path: "emergency",
    component: EmergencyContactsPage,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Emergency Contacts",
      permission: Permission.Safety.View,
    },
  },
];
