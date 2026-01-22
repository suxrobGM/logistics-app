import type { Routes } from "@angular/router";
import { DemoRequestsList } from "./demo-requests-list/demo-requests-list";

export const demoRequestsRoutes: Routes = [
  {
    path: "",
    component: DemoRequestsList,
    data: {
      breadcrumb: "Demo Requests",
    },
  },
];
