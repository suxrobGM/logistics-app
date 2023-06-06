export enum UserRole {
  AppSuperAdmin = 'app.superadmin',
  AppAdmin = 'app.admin',
  Owner = 'tenant.owner',
  Manager = 'tenant.manager',
  Dispatcher = 'tenant.dispatcher',
  Driver = 'tenant.driver'
}

export function getRoleName(role?: string): string | undefined {
  switch (role) {
    case UserRole.AppSuperAdmin:
      return 'Super Admin';

    case UserRole.AppAdmin:
      return 'Admin';

    case UserRole.Owner:
      return 'Owner';

    case UserRole.Manager:
      return 'Manager';

    case UserRole.Dispatcher:
      return 'Dispatcher';

    case UserRole.Driver:
      return 'Driver';
  }

  return undefined;
}
