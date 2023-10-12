import {UserRoles} from '../models';

export abstract class UserRoleHelper {
  static getRoleName(role?: string): string | null {
    switch (role) {
      case UserRoles.AppSuperAdmin: return 'Super Admin';
      case UserRoles.AppAdmin: return 'Admin';
      case UserRoles.Owner: return 'Owner';
      case UserRoles.Manager: return 'Manager';
      case UserRoles.Dispatcher: return 'Dispatcher';
      case UserRoles.Driver: return 'Driver';
    }

    return null;
  }
}
