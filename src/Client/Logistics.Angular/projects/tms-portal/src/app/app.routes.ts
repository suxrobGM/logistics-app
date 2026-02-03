import type { Routes } from "@angular/router";
import { featureGuardFromData } from "@logistics/shared";
import { NotFoundComponent, UnauthorizedComponent } from "@/pages/errors";
import { LoginComponent } from "@/pages/login";

export const appRoutes: Routes = [
  {
    path: "home",
    loadChildren: () => import("./pages/home/home.routes").then((m) => m.homeRoutes),
  },
  {
    path: "reports",
    loadChildren: () => import("./pages/reports/reports.routes").then((m) => m.reportsRoutes),
    canActivate: [featureGuardFromData],
    data: { breadcrumb: "Reports", feature: "reports" },
  },
  {
    path: "dashboard",
    loadChildren: () => import("./pages/dashboard/dashboard.routes").then((m) => m.dashboardRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Dashboard",
      feature: "dashboard",
    },
  },
  {
    path: "employees",
    loadChildren: () => import("./pages/employees/employee.routes").then((m) => m.employeeRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Employees",
      feature: "employees",
    },
  },
  {
    path: "loads",
    loadChildren: () => import("./pages/loads/load.routes").then((m) => m.loadRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Loads",
      feature: "loads",
    },
  },
  {
    path: "trucks",
    loadChildren: () => import("./pages/trucks/truck.routes").then((m) => m.truckRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Trucks",
      feature: "trucks",
    },
  },
  {
    path: "customers",
    loadChildren: () => import("./pages/customers/customer.routes").then((m) => m.customerRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Customers",
      feature: "customers",
    },
  },
  {
    path: "payment",
    loadChildren: () => import("./pages/payment/payment.routes").then((m) => m.paymentRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "",
      feature: "payments",
    },
  },
  {
    path: "invoices",
    loadChildren: () => import("./pages/invoices/invoice.routes").then((m) => m.invoiceRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "",
      feature: "invoices",
    },
  },
  {
    path: "settings",
    loadChildren: () => import("./pages/settings/settings.routes").then((m) => m.settingsRoutes),
    data: {
      breadcrumb: "Settings",
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
    canActivate: [featureGuardFromData],
    data: { feature: "trips" },
  },
  {
    path: "eld",
    loadChildren: () => import("./pages/eld/eld.routes").then((m) => m.eldRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "ELD / HOS",
      feature: "eld",
    },
  },
  {
    path: "loadboard",
    loadChildren: () =>
      import("./pages/load-board/load-board.routes").then((m) => m.loadBoardRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Load Board",
      feature: "loadboard",
    },
  },
  {
    path: "messages",
    loadChildren: () => import("./pages/messages/messages.routes").then((m) => m.messagesRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Messages",
      feature: "messages",
    },
  },
  {
    path: "notifications",
    loadChildren: () =>
      import("./pages/notifications/notifications.routes").then((m) => m.notificationsRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Notifications",
      feature: "notifications",
    },
  },
  {
    path: "safety",
    loadChildren: () => import("./pages/safety/safety.routes").then((m) => m.inspectionRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Safety",
      feature: "safety",
    },
  },
  {
    path: "expenses",
    loadChildren: () => import("./pages/expenses/expense.routes").then((m) => m.expenseRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Expenses",
      feature: "expenses",
    },
  },
  {
    path: "payroll",
    loadChildren: () => import("./pages/payroll/payroll.routes").then((m) => m.payrollRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Payroll",
      feature: "payroll",
    },
  },
  {
    path: "timesheets",
    loadChildren: () =>
      import("./pages/timesheets/timesheets.routes").then((m) => m.timesheetsRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Timesheets",
      feature: "timesheets",
    },
  },
  {
    path: "maintenance",
    loadChildren: () =>
      import("./pages/maintenance/maintenance.routes").then((m) => m.maintenanceRoutes),
    canActivate: [featureGuardFromData],
    data: {
      breadcrumb: "Maintenance",
      feature: "maintenance",
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
    component: NotFoundComponent,
  },
  {
    path: "**",
    redirectTo: "404",
  },
];
