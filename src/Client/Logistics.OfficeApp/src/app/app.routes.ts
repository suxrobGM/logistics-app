import {Routes} from "@angular/router";
import {Error404Component} from "@/pages/error404";
import {LoginComponent} from "@/pages/login";
import {UnauthorizedComponent} from "@/pages/unauthorized";

export const appRoutes: Routes = [
  {
    path: "home",
    loadChildren: () => import("./pages/home/home.routes").then((m) => m.homeRoutes),
  },
  {
    path: "dashboard",
    loadChildren: () => import("./pages/dashboard/dashboard.routes").then((m) => m.dashboardRoutes),
    data: {
      breadcrumb: "Dashboard",
    },
  },
  {
    path: "employees",
    loadChildren: () => import("./pages/employee/employee.routes").then((m) => m.employeeRoutes),
    data: {
      breadcrumb: "Employees",
    },
  },
  {
    path: "loads",
    loadChildren: () => import("./pages/load/load.routes").then((m) => m.loadRoutes),
    data: {
      breadcrumb: "Loads",
    },
  },
  {
    path: "trucks",
    loadChildren: () => import("./pages/truck/truck.routes").then((m) => m.truckRoutes),
    data: {
      breadcrumb: "Trucks",
    },
  },
  {
    path: "customers",
    loadChildren: () => import("./pages/customer/customer.routes").then((m) => m.customerRoutes),
    data: {
      breadcrumb: "Customers",
    },
  },
  {
    path: "payment",
    loadChildren: () => import("./pages/payment/payment.routes").then((m) => m.paymentRoutes),
    data: {
      breadcrumb: "",
    },
  },
  {
    path: "invoices",
    loadChildren: () => import("./pages/invoice/invoice.routes").then((m) => m.invoiceRoutes),
    data: {
      breadcrumb: "",
    },
  },
  {
    path: "subscription",
    loadChildren: () =>
      import("./pages/subscription/subscription.routes").then((m) => m.subscriptionRoutes),
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
