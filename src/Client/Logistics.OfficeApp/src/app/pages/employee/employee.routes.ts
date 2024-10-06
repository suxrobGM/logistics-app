import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {authGuard} from "@/core/guards";
import {AddEmployeeComponent} from "./add-employee/add-employee.component";
import {EditEmployeeComponent} from "./edit-employee/edit-employee.component";
import {ListEmployeeComponent} from "./list-employees/list-employees.component";

export const EMPLOYEE_ROUTES: Routes = [
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
