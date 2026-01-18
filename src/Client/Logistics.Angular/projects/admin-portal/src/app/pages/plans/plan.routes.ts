import type { Routes } from "@angular/router";
import { PlanAdd } from "./plan-add/plan-add";
import { PlanEdit } from "./plan-edit/plan-edit";
import { PlansList } from "./plans-list/plans-list";

export const planRoutes: Routes = [
  {
    path: "",
    component: PlansList,
    data: {
      breadcrumb: "Subscription Plans",
    },
  },
  {
    path: "add",
    component: PlanAdd,
    data: {
      breadcrumb: "Add",
    },
  },
  {
    path: ":id/edit",
    component: PlanEdit,
    data: {
      breadcrumb: "Edit",
    },
  },
];
