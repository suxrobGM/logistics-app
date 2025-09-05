import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/shared/models";
import {EmployeeAddComponent} from "./employee-add/employee-add";
import {EmployeeEditComponent} from "./employee-edit/employee-edit";
import {EmployeeListComponent} from "./employees-list/employees-list";

export const employeeRoutes: Routes = [
  {
    path: "",
    component: EmployeeListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Employees.View,
    },
  },
  {
    path: "add",
    component: EmployeeAddComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Employees.Create,
    },
  },
  {
    path: ":id/edit",
    component: EmployeeEditComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Employees.Edit,
    },
  },
];
