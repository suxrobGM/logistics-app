export const Permission = {
  AppRole: {
    View: "Permission.AppRole.View",
    Manage: "Permission.AppRole.Manage",
  },
  Employee: {
    View: "Permission.Employee.View",
    Manage: "Permission.Employee.Manage",
  },
  Load: {
    View: "Permission.Load.View",
    Manage: "Permission.Load.Manage",
  },
  Notification: {
    View: "Permission.Notification.View",
    Manage: "Permission.Notification.Manage",
  },
  Stat: {
    View: "Permission.Stat.View",
  },
  Tenant: {
    View: "Permission.Tenant.View",
    Manage: "Permission.Tenant.Manage",
  },
  TenantRole: {
    View: "Permission.TenantRole.View",
    Manage: "Permission.TenantRole.Manage",
  },
  Truck: {
    View: "Permission.Truck.View",
    Manage: "Permission.Truck.Manage",
  },
  User: {
    View: "Permission.User.View",
    Manage: "Permission.User.Manage",
  },
  Customer: {
    View: "Permission.Customer.View",
    Manage: "Permission.Customer.Manage",
  },
  Payment: {
    View: "Permission.Payment.View",
    Manage: "Permission.Payment.Manage",
  },
  Invoice: {
    View: "Permission.Invoice.View",
    Manage: "Permission.Invoice.Manage",
  },
  Payroll: {
    View: "Permission.Payroll.View",
    Manage: "Permission.Payroll.Manage",
  },
  Eld: {
    View: "Permission.Eld.View",
    Manage: "Permission.Eld.Manage",
    Sync: "Permission.Eld.Sync",
  },
  Message: {
    View: "Permission.Message.View",
    Manage: "Permission.Message.Manage",
  },
  Invitation: {
    View: "Permission.Invitation.View",
    Manage: "Permission.Invitation.Manage",
  },
  Portal: {
    Access: "Permission.Portal.Access",
    ViewLoads: "Permission.Portal.ViewLoads",
    ViewInvoices: "Permission.Portal.ViewInvoices",
    ViewDocuments: "Permission.Portal.ViewDocuments",
  },
  Expense: {
    View: "Permission.Expense.View",
    Manage: "Permission.Expense.Manage",
  },
  LoadBoard: {
    View: "Permission.LoadBoard.View",
    Search: "Permission.LoadBoard.Search",
    Book: "Permission.LoadBoard.Book",
    Post: "Permission.LoadBoard.Post",
    Manage: "Permission.LoadBoard.Manage",
  },
  BlogPost: {
    View: "Permission.BlogPost.View",
    Manage: "Permission.BlogPost.Manage",
  },
  Dvir: {
    View: "Permission.Dvir.View",
    Manage: "Permission.Dvir.Manage",
    Review: "Permission.Dvir.Review",
  },
  Safety: {
    View: "Permission.Safety.View",
    Manage: "Permission.Safety.Manage",
  },
  Maintenance: {
    View: "Permission.Maintenance.View",
    Manage: "Permission.Maintenance.Manage",
  },
  Accident: {
    View: "Permission.Accident.View",
    Manage: "Permission.Accident.Manage",
    Review: "Permission.Accident.Review",
  },
} as const;

/**
 * Type representing all possible permission values.
 * Extracted from the Permission object for strong typing.
 */
export type PermissionValue = {
  [K in keyof typeof Permission]: (typeof Permission)[K][keyof (typeof Permission)[K]];
}[keyof typeof Permission];
