import { Component, computed, inject, input } from "@angular/core";
import type { PermissionValue } from "@logistics/shared/models";
import { PERMISSION_CHECKER } from "./permission-checker";

/**
 * A reusable component that conditionally renders its content based on user permissions.
 *
 * Usage:
 * ```html
 * <lib-permission-guard [permissions]="'Permission.Employee.Manage'">
 *   <button>Invite Employee</button>
 * </lib-permission-guard>
 *
 * <lib-permission-guard [permissions]="['Permission.Employee.View', 'Permission.Employee.Manage']" mode="any">
 *   <button>View or Manage</button>
 * </lib-permission-guard>
 * ```
 *
 * The consuming application must provide the PERMISSION_CHECKER token:
 * ```typescript
 * providers: [
 *   { provide: PERMISSION_CHECKER, useFactory: () => inject(PermissionService) }
 * ]
 * ```
 */
@Component({
  selector: "lib-permission-guard",
  template: `
    @if (hasAccess()) {
      <ng-content />
    }
  `,
})
export class PermissionGuard {
  private readonly checker = inject(PERMISSION_CHECKER, { optional: true });

  /**
   * Permission(s) to check. Can be a single permission or array of permissions.
   */
  public readonly permissions = input.required<PermissionValue | PermissionValue[]>();

  /**
   * Mode for checking multiple permissions:
   * - 'any': User needs at least one of the permissions (default)
   * - 'all': User needs all of the permissions
   */
  public readonly mode = input<"any" | "all">("any");

  protected readonly hasAccess = computed(() => {
    if (!this.checker) {
      console.warn("PermissionGuard: No PERMISSION_CHECKER provided. Content will be hidden.");
      return false;
    }

    const perms = this.permissions();
    const permArray = Array.isArray(perms) ? perms : [perms];
    const checkMode = this.mode();

    if (checkMode === "all") {
      return this.checker.hasAllPermissions(...permArray);
    }
    return this.checker.hasAnyPermission(...permArray);
  });
}
