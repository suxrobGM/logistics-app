import { Injectable, inject, signal } from "@angular/core";
import { Api } from "../api/generated/api";
import { getCurrentUserPermissions } from "../api/generated/functions";
import type { PermissionChecker } from "../components/permission/permission-checker";

/**
 * Service for managing user permissions.
 * Fetches permissions from the API and provides methods to check them.
 *
 * @example
 * ```typescript
 * // In app.config.ts
 * providers: [
 *   { provide: PERMISSION_CHECKER, useExisting: PermissionService }
 * ]
 *
 * // In a component
 * const permissionService = inject(PermissionService);
 * await permissionService.loadPermissions();
 * if (permissionService.hasPermission('users.view')) {
 *   // Show users section
 * }
 * ```
 */
@Injectable({ providedIn: "root" })
export class PermissionService implements PermissionChecker {
  private readonly api = inject(Api);

  private readonly _permissions = signal<string[]>([]);
  private loaded = false;

  /**
   * Readonly signal of the current permissions array.
   */
  public readonly permissions = this._permissions.asReadonly();

  /**
   * Load permissions from the API.
   * Caches the result and won't reload unless clearPermissions() is called first.
   */
  async loadPermissions(): Promise<void> {
    if (this.loaded) {
      return;
    }

    try {
      const permissions = await this.api.invoke(getCurrentUserPermissions);
      this._permissions.set(permissions);
      this.loaded = true;
    } catch {
      // If loading permissions fails (e.g., 401), use empty permissions
      // Authorization is still validated server-side
      this._permissions.set([]);
    }
  }

  /**
   * Check if user has a specific permission.
   */
  hasPermission(permission: string): boolean {
    return this._permissions().includes(permission);
  }

  /**
   * Check if user has at least one of the specified permissions.
   */
  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some((p) => this._permissions().includes(p));
  }

  /**
   * Check if user has all of the specified permissions.
   */
  hasAllPermissions(...permissions: string[]): boolean {
    return permissions.every((p) => this._permissions().includes(p));
  }

  /**
   * Clear the cached permissions. Next call to loadPermissions() will fetch fresh data.
   */
  clearPermissions(): void {
    this._permissions.set([]);
    this.loaded = false;
  }
}
