import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { EmployeeAdd } from "./employee-add/employee-add";
import { EmployeeDocumentsPage } from "./employee-documents/employee-documents";
import { EmployeeEdit } from "./employee-edit/employee-edit";
import { EmployeeList } from "./employees-list/employees-list";
import { PendingInvitations } from "./pending-invitations/pending-invitations";

export const employeeRoutes: Routes = [
  {
    path: "",
    component: EmployeeList,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Employee.View,
    },
  },
  {
    path: "add",
    component: EmployeeAdd,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Employee.Manage,
    },
  },
  {
    path: "invitations",
    component: PendingInvitations,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Invitations",
      permission: Permission.Invitation.Manage,
    },
  },
  {
    path: ":id/edit",
    component: EmployeeEdit,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permission.Employee.Manage,
    },
  },
  {
    path: ":id/documents",
    component: EmployeeDocumentsPage,
    // canActivate: [authGuard],
    data: {
      breadcrumb: "Documents",
      // permission: Permission.Employee.Manage,
    },
  },
];
