import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared";
import { authGuard } from "@/core/auth";
import { EmployeeAdd } from "./employee-add/employee-add";
import { EmployeeDetails } from "./employee-details/employee-details";
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
    path: ":id",
    component: EmployeeDetails,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Details",
      permission: Permission.Employee.View,
    },
  },
];
