import type { Routes } from "@angular/router";
import { IftaRateAdd } from "./ifta-rate-add/ifta-rate-add";
import { IftaRateEdit } from "./ifta-rate-edit/ifta-rate-edit";
import { IftaRatesList } from "./ifta-rates-list/ifta-rates-list";

export const iftaRatesRoutes: Routes = [
  {
    path: "",
    component: IftaRatesList,
    data: { breadcrumb: "IFTA Tax Rates" },
  },
  {
    path: "add",
    component: IftaRateAdd,
    data: { breadcrumb: "Add" },
  },
  {
    path: ":id/edit",
    component: IftaRateEdit,
    data: { breadcrumb: "Edit" },
  },
];
