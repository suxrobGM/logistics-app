import type { SelectOption } from "./select-option";

export enum UserRole {
  AppSuperAdmin = "app.superadmin",
  AppAdmin = "app.admin",
  Owner = "tenant.owner",
  Manager = "tenant.manager",
  Dispatcher = "tenant.dispatcher",
  Driver = "tenant.driver",
}

export const userRoleOptions: SelectOption<UserRole>[] = [
  { label: "Super Admin", value: UserRole.AppSuperAdmin },
  { label: "Admin", value: UserRole.AppAdmin },
  { label: "Owner", value: UserRole.Owner },
  { label: "Manager", value: UserRole.Manager },
  { label: "Dispatcher", value: UserRole.Dispatcher },
  { label: "Driver", value: UserRole.Driver },
] as const;
