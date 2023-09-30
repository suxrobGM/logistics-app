export namespace Permissions {
  export enum Employee {
    Create = 'Permissions.Employee.Create',
    View = 'Permissions.Employee.View',
    Edit = 'Permissions.Employee.Edit',
    Delete = 'Permissions.Employee.Delete'
  }

  export enum Load {
    Create = 'Permissions.Load.Create',
    View = 'Permissions.Load.View',
    Edit = 'Permissions.Load.Edit',
    Delete = 'Permissions.Load.Delete'
  }

  export enum Stats {
    Create = `Permissions.Stats.Create`,
    View = 'Permissions.Stats.View',
    Edit = 'Permissions.Stats.Edit',
    Delete = 'Permissions.Stats.Delete'
  }

  export enum TenantRole {
    Create = 'Permissions.TenantRole.Create',
    View = 'Permissions.TenantRole.View',
    Edit = 'Permissions.TenantRole.Edit',
    Delete = 'Permissions.TenantRole.Delete'
  }

  export enum Truck {
    Create = 'Permissions.Truck.Create',
    View = 'Permissions.Truck.View',
    Edit = 'Permissions.Truck.Edit',
    Delete = 'Permissions.Truck.Delete'
  }
}
