import {
  EnumLike,
  EnumType,
  convertEnumToArray,
  findValueFromEnum,
} from './enumLike';

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

  getValue(enumValue: string | number): EnumType {
    return findValueFromEnum(this, enumValue);
  },

  toArray(): EnumType[] {
    return convertEnumToArray(this);
  },
};
