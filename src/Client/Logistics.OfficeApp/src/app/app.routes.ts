import {Routes} from "@angular/router";
import {Error404Component} from "@/pages/error404";
import {LoginComponent} from "@/pages/login";
import {UnauthorizedComponent} from "@/pages/unauthorized";

export const appRoutes: Routes = [
  {
    path: "home",
    loadChildren: () => import("./pages/home").then((m) => m.HOME_ROUTES),
  },
  {
    path: "dashboard",
    loadChildren: () => import("./pages/dashboard").then((m) => m.DASHBOARD_ROUTES),
    data: {
      breadcrumb: "Dashboard",
    },
  },
  {
    path: "employees",
    loadChildren: () => import("./pages/employee").then((m) => m.employeeRoutes),
    data: {
      breadcrumb: "Employees",
    },
  },
  {
    path: "loads",
    loadChildren: () => import("./pages/load").then((m) => m.loadRoutes),
    data: {
      breadcrumb: "Loads",
    },
  },
  {
    path: "trucks",
    loadChildren: () => import("./pages/truck").then((m) => m.truckRoutes),
    data: {
      breadcrumb: "Trucks",
    },
  },
  {
    path: "customers",
    loadChildren: () => import("./pages/customer").then((m) => m.customerRoutes),
    data: {
      breadcrumb: "Customers",
    },
  },
  {
    path: "accounting",
    loadChildren: () => import("./pages/accounting").then((m) => m.accountingRoutes),
    data: {
      breadcrumb: "",
    },
  },
  {
    path: "payment",
    loadChildren: () => import("./pages/payment").then((m) => m.paymentRoutes),
    data: {
      breadcrumb: "",
    },
  },
  {
    path: "",
    component: LoginComponent,
  },
  {
    path: "unauthorized",
    component: UnauthorizedComponent,
  },
  {
    path: "404",
    component: Error404Component,
  },
  {
    path: "**",
    redirectTo: "404",
  },
];
