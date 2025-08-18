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
    path: "reports",
    loadChildren: () => import("./pages/reports/reports.routes").then(m => m.reportsRoutes),
    data: { breadcrumb: "Reports" }
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
    loadChildren: () => import("./pages/employees/employee.routes").then((m) => m.employeeRoutes),
    data: {
      breadcrumb: "Employees",
    },
  },
  {
    path: "loads",
    loadChildren: () => import("./pages/loads/load.routes").then((m) => m.loadRoutes),
    data: {
      breadcrumb: "Loads",
    },
  },
  {
    path: "trucks",
    loadChildren: () => import("./pages/trucks/truck.routes").then((m) => m.truckRoutes),
    data: {
      breadcrumb: "Trucks",
    },
  },
  {
    path: "customers",
    loadChildren: () => import("./pages/customers/customer.routes").then((m) => m.customerRoutes),
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
    loadChildren: () => import("./pages/invoices/invoice.routes").then((m) => m.invoiceRoutes),
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
    path: "trips",
    loadChildren: () => import("./pages/trips/trip.routes").then((m) => m.tripRoutes),
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
