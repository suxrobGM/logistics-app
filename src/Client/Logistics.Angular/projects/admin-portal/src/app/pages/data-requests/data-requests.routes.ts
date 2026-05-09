import type { Routes } from "@angular/router";
import { DataRequestsList } from "./data-requests-list/data-requests-list";

export const dataRequestsRoutes: Routes = [
  {
    path: "",
    component: DataRequestsList,
    data: {
      breadcrumb: "Data Requests",
    },
  },
];
