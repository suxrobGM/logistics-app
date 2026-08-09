import { computed, inject, Injectable, signal } from "@angular/core";
import { Api } from "../api/generated/api";
import { getCurrentUserPermissions } from "../api/generated/functions";
import type { PermissionChecker } from "../permission/permission-checker";

/**
 * Loads the current user's permissions once and answers checks against them.
 * Provide it as `PERMISSION_CHECKER` in the portal's `app.config.ts`.
 */
@Injectable({ providedIn: "root" })
export class PermissionService implements PermissionChecker {
  private readonly api = inject(Api);

  private readonly _permissions = signal<string[]>([]);
  private readonly state = signal<"idle" | "loaded" | "failed">("idle");

  /**
   * Readonly signal of the current permissions array.
   */
  public readonly permissions = this._permissions.asReadonly();

  /**
   * Whether the first {@link loadPermissions} attempt has finished, successfully or not. Anything
   * that persists permission-derived state must wait: until then every check answers false.
   */
  public readonly resolved = computed(() => this.state() !== "idle");

  /**
   * Load permissions from the API.
   * Caches the result and won't reload unless clearPermissions() is called first.
   */
  async loadPermissions(): Promise<void> {
    if (this.state() === "loaded") {
      return;
    }

    try {
      this._permissions.set(await this.api.invoke(getCurrentUserPermissions));
      this.state.set("loaded");
    } catch {
      // Fail closed (e.g. on a 401) and allow a later retry; the API re-checks every permission.
      this._permissions.set([]);
      this.state.set("failed");
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
    this.state.set("idle");
  }
}
