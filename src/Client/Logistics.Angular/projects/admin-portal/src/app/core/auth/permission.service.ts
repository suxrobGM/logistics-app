import { Injectable, inject } from "@angular/core";
import { Api, getCurrentUserPermissions } from "@logistics/shared/api";
import type { PermissionChecker } from "@logistics/shared";

@Injectable({ providedIn: "root" })
export class PermissionService implements PermissionChecker {
  private readonly api = inject(Api);
  private permissions: string[] = [];

  async loadPermissions(): Promise<void> {
    try {
      this.permissions = await this.api.invoke(getCurrentUserPermissions);
    } catch {
      // If loading permissions fails (e.g., 401), use empty permissions
      // SuperAdmin users will still function as they're validated server-side
      this.permissions = [];
    }
  }

  hasPermission(permission: string): boolean {
    return this.permissions.includes(permission);
  }

  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some((p) => this.permissions.includes(p));
  }

  hasAllPermissions(...permissions: string[]): boolean {
    return permissions.every((p) => this.permissions.includes(p));
  }

  clearPermissions(): void {
    this.permissions = [];
  }
}
