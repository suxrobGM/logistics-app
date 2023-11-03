import {EnumLike, getEnumDescription} from './enumLike';

export enum UserRole {
  AppSuperAdmin = 'app.superadmin',
  AppAdmin = 'app.admin',
  Owner = 'tenant.owner',
  Manager = 'tenant.manager',
  Dispatcher = 'tenant.dispatcher',
  Driver = 'tenant.driver'
}

export const UserRoleEnum: EnumLike = {
  AppSuperAdmin: {value: 'app.superadmin', description: 'Super Admin'},
  AppAdmin: {value: 'app.admin', description: 'Admin'},
  Owner: {value: 'tenant.owner', description: 'Owner'},
  Manager: {value: 'tenant.manager', description: 'Manager'},
  Dispatcher: {value: 'tenant.dispatcher', description: 'Dispatcher'},
  Driver: {value: 'tenant.driver', description: 'Driver'},

  getDescription(value: string | number): string {
    return getEnumDescription(this, value);
  },
};
