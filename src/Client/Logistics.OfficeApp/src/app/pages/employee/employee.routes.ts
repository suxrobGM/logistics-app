import {Routes} from "@angular/router";
import {Permissions} from "@/core/enums";
import {AuthGuard} from "@/core/guards";
import {AddEmployeeComponent} from "./add-employee/add-employee.component";
import {EditEmployeeComponent} from "./edit-employee/edit-employee.component";
import {ListEmployeeComponent} from "./list-employees/list-employees.component";

export const EMPLOYEE_ROUTES: Routes = [
  {
    path: "",
    component: ListEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Employees.View,
    },
  },
  {
    path: "add",
    component: AddEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Employees.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditEmployeeComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Employees.Edit,
    },
  },
];
