import type { Routes } from "@angular/router";
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
    data: { breadcrumb: "Reports" },
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
  },
  {
    path: "eld",
    loadChildren: () => import("./pages/eld/eld.routes").then((m) => m.eldRoutes),
    data: {
      breadcrumb: "ELD / HOS",
    },
  },
  {
    path: "loadboard",
    loadChildren: () =>
      import("./pages/load-board/load-board.routes").then((m) => m.loadBoardRoutes),
    data: {
      breadcrumb: "Load Board",
    },
  },
  {
    path: "messages",
    loadChildren: () => import("./pages/messages/messages.routes").then((m) => m.messagesRoutes),
    data: {
      breadcrumb: "Messages",
    },
  },
  {
    path: "inspections",
    loadChildren: () =>
      import("./pages/inspections/inspection.routes").then((m) => m.inspectionRoutes),
    data: {
      breadcrumb: "Inspections",
    },
  },
  {
    path: "expenses",
    loadChildren: () => import("./pages/expenses/expense.routes").then((m) => m.expenseRoutes),
    data: {
      breadcrumb: "Expenses",
    },
  },
  {
    path: "time-entries",
    loadChildren: () =>
      import("./pages/time-entries/time-entry.routes").then((m) => m.timeEntryRoutes),
    data: {
      breadcrumb: "Time Entries",
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
