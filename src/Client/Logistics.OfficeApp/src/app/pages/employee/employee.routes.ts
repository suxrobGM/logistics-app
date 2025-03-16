import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {AddEmployeeComponent} from "./add-employee/add-employee.component";
import {EditEmployeeComponent} from "./edit-employee/edit-employee.component";
import {ListEmployeeComponent} from "./list-employees/list-employees.component";

export const employeeRoutes: Routes = [
  {
    path: "",
    component: ListEmployeeComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Employees.View,
    },
  },
  {
    path: "add",
    component: AddEmployeeComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Employees.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditEmployeeComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Employees.Edit,
    },
  },
];
