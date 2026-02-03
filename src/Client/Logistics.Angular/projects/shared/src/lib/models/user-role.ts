import type { SelectOption } from "./select-option";

export enum UserRole {
  AppSuperAdmin = "app.superadmin",
  AppAdmin = "app.admin",
  AppManager = "app.manager",
  Owner = "tenant.owner",
  Manager = "tenant.manager",
  Dispatcher = "tenant.dispatcher",
  Driver = "tenant.driver",
  Customer = "tenant.customer",
}

export const userRoleOptions: SelectOption<UserRole>[] = [
  { label: "Super Admin", value: UserRole.AppSuperAdmin },
  { label: "Admin", value: UserRole.AppAdmin },
  { label: "App Manager", value: UserRole.AppManager },
  { label: "Owner", value: UserRole.Owner },
  { label: "Manager", value: UserRole.Manager },
  { label: "Dispatcher", value: UserRole.Dispatcher },
  { label: "Driver", value: UserRole.Driver },
  { label: "Customer", value: UserRole.Customer },
] as const;
