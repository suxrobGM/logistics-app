/* eslint-disable @typescript-eslint/no-namespace */

export namespace Permission {
  export enum Employee {
    View = "Permission.Employee.View",
    Manage = "Permission.Employee.Manage",
  }

  export enum Load {
    View = "Permission.Load.View",
    Manage = "Permission.Load.Manage",
  }

  export enum Notification {
    View = "Permission.Notification.View",
    Manage = "Permission.Notification.Manage",
  }

  export enum Stat {
    View = "Permission.Stat.View",
  }

  export enum TenantRole {
    View = "Permission.TenantRole.View",
    Manage = "Permission.TenantRole.Manage",
  }

  export enum Truck {
    View = "Permission.Truck.View",
    Manage = "Permission.Truck.Manage",
  }

  export enum Customer {
    View = "Permission.Customer.View",
    Manage = "Permission.Customer.Manage",
  }

  export enum Payment {
    View = "Permission.Payment.View",
    Manage = "Permission.Payment.Manage",
  }

  export enum Invoice {
    View = "Permission.Invoice.View",
    Manage = "Permission.Invoice.Manage",
  }

  export enum Payroll {
    View = "Permission.Payroll.View",
    Manage = "Permission.Payroll.Manage",
  }

  export enum Eld {
    View = "Permission.Eld.View",
    Manage = "Permission.Eld.Manage",
    Sync = "Permission.Eld.Sync",
  }
}
