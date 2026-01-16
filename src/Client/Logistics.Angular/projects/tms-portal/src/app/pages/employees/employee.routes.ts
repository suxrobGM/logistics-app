import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared/models";
import { authGuard } from "@/core/auth";
import { EmployeeAddComponent } from "./employee-add/employee-add";
import { EmployeeDocumentsPage } from "./employee-documents/employee-documents";
import { EmployeeEditComponent } from "./employee-edit/employee-edit";
import { EmployeeListComponent } from "./employees-list/employees-list";

export const employeeRoutes: Routes = [
  {
    path: "",
    component: EmployeeListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Employee.View,
    },
  },
  {
    path: "add",
    component: EmployeeAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permission.Employee.Manage,
    },
  },
  {
    path: ":id/edit",
    component: EmployeeEditComponent,
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
