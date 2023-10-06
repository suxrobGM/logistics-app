import {UserRole} from '../models';

export abstract class UserRoleHelper {
  static getRoleName(role?: string): string | null {
    switch (role) {
      case UserRole.AppSuperAdmin: return 'Super Admin';
      case UserRole.AppAdmin: return 'Admin';
      case UserRole.Owner: return 'Owner';
      case UserRole.Manager: return 'Manager';
      case UserRole.Dispatcher: return 'Dispatcher';
      case UserRole.Driver: return 'Driver';
    }

    return null;
  }
}
